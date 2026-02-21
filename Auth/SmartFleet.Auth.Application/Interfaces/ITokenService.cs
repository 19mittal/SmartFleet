using SmartFleet.Auth.Domain.Entities;

namespace SmartFleet.Auth.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
