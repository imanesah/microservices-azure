using AuthService.Models;

namespace AuthService.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
