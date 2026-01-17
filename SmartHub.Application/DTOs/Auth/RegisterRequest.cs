namespace SmartHub.Application.DTOs.Auth
{
    // DTO for user registration requests
    public class RegisterRequest
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
