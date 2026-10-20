using TodoAPI.Models;

namespace TodoAPI.Abstractions
{
    public interface ITokenService
    {
        TokenModel GenerateToken(User user);
    }
}
