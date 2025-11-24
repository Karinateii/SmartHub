using SmartHub.Domain.Common;
using SmartHub.Domain.Enums;

namespace SmartHub.Domain.Entities
{
  public class User : AuditableEntity
  {
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;
    public Role Role { get; set; } = Role.User;
    
    public string? ProfileImagedUrl { get; set; }

    public bool EmailVerified { get; set; } = false;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
  }
}

/* This covers everything an authentication system needs:
1.User Identity
2. PasswordHash
3. Roles
4. Email verification
5. Refresh tokens
6. Profile image
*/