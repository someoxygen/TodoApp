using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoAPI.Abstractions;
using TodoAPI.Data;
using TodoAPI.DTOs.Auth;
using TodoAPI.Models;

namespace TodoAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthService(AppDbContext context, IPasswordHasher<User> passwordHasher, ITokenService tokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }


        public async Task<AuthResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if(user is null)
            {
                throw new InvalidOperationException("Invalid email or password.");
            }

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if(passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                throw new InvalidOperationException("Invalid email or password.");
            }

            return CreateAuthResponse(user);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var existingUser = await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
            if(existingUser)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = email,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return CreateAuthResponse(user);
        }

        private AuthResponseDto CreateAuthResponse(User user)
        {
            var jwt = _tokenService.GenerateToken(user);
            return new AuthResponseDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Token = jwt.TokenValue,
                ExpiresAt = jwt.ExpiresAt
            };
        }
    }
}
