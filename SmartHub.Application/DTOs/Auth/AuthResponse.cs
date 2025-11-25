namespace SmartHub.Application.DTOs.Auth
{
  //This DTO returns JWT + User info after login
  public class AuthResponse
  {
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = default!;
    public DateTime RefreshTokenExpiry { get; set; }

    //User details
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string? ProfileImageUrl { get; set; }
  }
}