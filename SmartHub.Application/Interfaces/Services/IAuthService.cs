using SmartHub.Application.DTOs.Auth;

namespace SmartHub.Application.Interfaces.Services
{
  // Interface for authentication services
  public interface IAuthService
  {
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenByUserIdAsync(Guid userId);
  }
}