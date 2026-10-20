using Microsoft.EntityFrameworkCore;
using TodoAPI.Models;

namespace TodoAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Todo> Todos => Set<Todo>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Todo>(x =>
            {
                x.HasKey(t => t.Id);
                x.Property(t => t.Title).IsRequired();
                x.HasIndex(x => x.Title).IsUnique();
            });
            modelBuilder.Entity<User>(x =>
            {
                x.HasKey(u => u.Id);
                x.Property(u => u.FullName).IsRequired();
                x.Property(u => u.PasswordHash).IsRequired();
                x.Property(u => u.Email).IsRequired();
                x.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}
