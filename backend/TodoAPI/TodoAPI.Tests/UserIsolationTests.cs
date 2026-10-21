using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using TodoAPI.Data;
using TodoAPI.DTOs.Auth;
using TodoAPI.DTOs.Todos;
using TodoAPI.Models;

namespace TodoAPI.Tests;

public class UserIsolationTests
{
    [Theory]
    [InlineData("https://todo.example.com", true)]
    [InlineData("https://other.example.com", false)]
    public async Task Deployment_health_is_public_and_cors_only_allows_configured_origin(string origin, bool allowed)
    {
        using var app = new TestApp();
        using var client = app.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", origin);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(allowed, response.Headers.Contains("Access-Control-Allow-Origin"));
        if (allowed)
            Assert.Equal(origin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
    }

    [Fact]
    public async Task Users_can_only_read_update_and_delete_their_own_todos()
    {
        using var app = new TestApp();
        using var alice = await Register(app, "alice");
        using var bob = await Register(app, "bob");

        // The same title is valid for different users, including on update.
        var aliceTodo = await CreateTodo(alice, "Ortak başlık");
        var bobTodo = await CreateTodo(bob, "Ortak başlık");
        Assert.Equal(new[] { aliceTodo.Id }, (await List(alice)).Select(t => t.Id));
        Assert.Equal(new[] { bobTodo.Id }, (await List(bob)).Select(t => t.Id));

        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync($"/api/Todos/{aliceTodo.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.PutAsJsonAsync($"/api/Todos/{aliceTodo.Id}",
            new { title = "Değiştirildi", isActive = false })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.DeleteAsync($"/api/Todos/{aliceTodo.Id}")).StatusCode);
        var unchanged = await alice.GetFromJsonAsync<TodoDto>($"/api/Todos/{aliceTodo.Id}");
        Assert.Equal("Ortak başlık", unchanged!.Title);
        Assert.True(unchanged.IsActive);

        Assert.Equal(HttpStatusCode.OK, (await bob.PutAsJsonAsync($"/api/Todos/{bobTodo.Id}",
            new { title = "Ortak başlık", isActive = false, createDate = bobTodo.CreateDate })).StatusCode);
        Assert.False((await bob.GetFromJsonAsync<TodoDto>($"/api/Todos/{bobTodo.Id}"))!.IsActive);
        Assert.Equal(HttpStatusCode.OK, (await bob.DeleteAsync($"/api/Todos/{bobTodo.Id}")).StatusCode);
        Assert.Empty(await List(bob));
        Assert.Single(await List(alice));
    }

    [Fact]
    public async Task Client_cannot_choose_or_change_the_owner_and_legacy_todos_stay_private()
    {
        using var app = new TestApp();
        using var alice = await Register(app, "alice");
        using var bob = await Register(app, "bob");
        int bobId;
        int legacyId;
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            bobId = await db.Users.Where(u => u.Email == "bob@example.com").Select(u => u.Id).SingleAsync();
            var legacy = new Todo { Title = "Eski görev" };
            db.Todos.Add(legacy);
            await db.SaveChangesAsync();
            legacyId = legacy.Id;
        }

        var response = await alice.PostAsJsonAsync("/api/Todos", new { title = "Kişisel görev", userId = bobId });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = (await response.Content.ReadFromJsonAsync<TodoDto>())!;
        Assert.Equal(HttpStatusCode.OK, (await alice.PutAsJsonAsync($"/api/Todos/{todo.Id}",
            new { title = todo.Title, userId = bobId, createDate = todo.CreateDate })).StatusCode);
        Assert.Single(await List(alice));
        Assert.Empty(await List(bob));
        foreach (var client in new[] { alice, bob })
        {
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/Todos/{legacyId}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/api/Todos/{legacyId}",
                new { title = "Sahiplen" })).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/Todos/{legacyId}")).StatusCode);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("FarkliSifre123!")]
    public async Task Registration_rejects_missing_or_mismatched_password_confirmation(string? confirmation)
    {
        using var app = new TestApp();
        using var client = app.CreateClient();
        var response = await client.PostAsJsonAsync("/api/Auth/register", new
        {
            fullName = "Test Kullanıcı", email = "test@example.com",
            password = "Sifre123!", confirmPassword = confirmation
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = app.Services.CreateScope();
        Assert.Empty(await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users.ToListAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("missing")]
    [InlineData("invalid")]
    [InlineData("0")]
    [InlineData("-1")]
    public async Task Todo_endpoints_reject_anonymous_or_invalid_user_identity(string? subject)
    {
        using var app = new TestApp();
        using var client = app.CreateClient();
        if (subject is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", app.Token(subject));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Todos")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Todos/1")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/Todos", new { title = "Test" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PutAsJsonAsync("/api/Todos/1", new { title = "Test" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.DeleteAsync("/api/Todos/1")).StatusCode);
    }

    private static async Task<HttpClient> Register(TestApp app, string name)
    {
        var client = app.CreateClient();
        var response = await client.PostAsJsonAsync("/api/Auth/register", new
        {
            fullName = name, email = $"{name}@example.com", password = "Sifre123!", confirmPassword = "Sifre123!"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponseDto>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return client;
    }

    private static async Task<TodoDto> CreateTodo(HttpClient client, string title)
    {
        var response = await client.PostAsJsonAsync("/api/Todos", new { title });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TodoDto>())!;
    }

    private static async Task<List<TodoDto>> List(HttpClient client) =>
        (await client.GetFromJsonAsync<List<TodoDto>>("/api/Todos"))!;
}

internal sealed class TestApp : WebApplicationFactory<Program>
{
    private const string Key = "test-only-signing-key-with-at-least-32-bytes";
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Cors:AllowedOrigins:0", "https://todo.example.com");
        builder.UseSetting("Database:ApplyMigrations", "false");
        builder.UseSetting("Jwt:Key", Key);
        builder.UseSetting("Jwt:Issuer", "TodoAPI.Tests");
        builder.UseSetting("Jwt:Audience", "TodoAPI.Tests");
        builder.UseSetting("Jwt:DurationInMinutes", "30");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            connection.Open();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }

    public string Token(string subject)
    {
        var claims = subject == "missing" ? Array.Empty<Claim>() : [new Claim("sub", subject)];
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: "TodoAPI.Tests", audience: "TodoAPI.Tests", claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                SecurityAlgorithms.HmacSha256)));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) connection.Dispose();
    }
}
