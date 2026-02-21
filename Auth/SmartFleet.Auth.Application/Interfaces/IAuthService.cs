using SmartFleet.Auth.Application.DTOs;

namespace SmartFleet.Auth.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> SignupAsync(SignupRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);
    }
}
