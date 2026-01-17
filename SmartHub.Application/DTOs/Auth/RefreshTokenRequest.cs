namespace SmartHub.Application.DTOs.Auth
{
    // DTO for refresh token requests
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = default!;
    }
}
