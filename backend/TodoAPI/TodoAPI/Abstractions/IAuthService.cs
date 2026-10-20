using TodoAPI.DTOs.Auth;

namespace TodoAPI.Abstractions
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
    }
}
