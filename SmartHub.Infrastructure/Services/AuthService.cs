using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartHub.Application.DTOs.Auth;
using SmartHub.Application.Interfaces.Services;
using SmartHub.Domain.Entities;
using SmartHub.Infrastructure.Persistence;

namespace SmartHub.Infrastructure.Services
{
  public class AuthService : IAuthService
  {
    private readonly SmartHubDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(SmartHubDbContext dbContext, IConfiguration configuration)
    {
      _dbContext = dbContext;
      _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
      // check duplicate email
      var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
      if (emailExists)
        throw new InvalidOperationException("A user with the provided email already exists.");

      var user = new User
      {
        Id = Guid.NewGuid(),
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        Role = SmartHub.Domain.Enums.Role.User,
        ProfileImagedUrl = request.ProfileImageUrl,
        EmailVerified = false
      };

      _dbContext.Users.Add(user);
      await _dbContext.SaveChangesAsync();

      var authResponse = CreateAuthResponse(user);
      var hashedRefresh = BCrypt.Net.BCrypt.HashPassword(authResponse.RefreshToken);
      user.RefreshToken = hashedRefresh;
      user.RefreshTokenExpiry = authResponse.RefreshTokenExpiry;
      await _dbContext.SaveChangesAsync();

      return authResponse;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
      var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
      if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        throw new InvalidOperationException("Invalid credentials.");

      var authResponse = CreateAuthResponse(user);
      var hashedRefresh2 = BCrypt.Net.BCrypt.HashPassword(authResponse.RefreshToken);
      user.RefreshToken = hashedRefresh2;
      user.RefreshTokenExpiry = authResponse.RefreshTokenExpiry;
      await _dbContext.SaveChangesAsync();

      return authResponse;
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
      // find users with a refresh token set and verify the provided token against the hashed value
      var usersWithToken = await _dbContext.Users.Where(u => u.RefreshToken != null).ToListAsync();
      var user = usersWithToken.FirstOrDefault(u => BCrypt.Net.BCrypt.Verify(refreshToken, u.RefreshToken));
      if (user == null || user.RefreshTokenExpiry == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
        throw new InvalidOperationException("Invalid or expired refresh token.");

      var authResponse = CreateAuthResponse(user);
      var hashedRefresh3 = BCrypt.Net.BCrypt.HashPassword(authResponse.RefreshToken);
      user.RefreshToken = hashedRefresh3;
      user.RefreshTokenExpiry = authResponse.RefreshTokenExpiry;
      await _dbContext.SaveChangesAsync();

      return authResponse;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
      var usersWithToken = await _dbContext.Users.Where(u => u.RefreshToken != null).ToListAsync();
      var user = usersWithToken.FirstOrDefault(u => BCrypt.Net.BCrypt.Verify(refreshToken, u.RefreshToken));
      if (user == null)
        throw new InvalidOperationException("Invalid refresh token.");

      user.RefreshToken = null;
      user.RefreshTokenExpiry = null;
      await _dbContext.SaveChangesAsync();
    }

    public async Task RevokeRefreshTokenByUserIdAsync(Guid userId)
    {
      var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
      if (user == null)
        throw new InvalidOperationException("User not found.");

      user.RefreshToken = null;
      user.RefreshTokenExpiry = null;
      await _dbContext.SaveChangesAsync();
    }

    private AuthResponse CreateAuthResponse(User user)
    {
      var key = _configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT Key not configured");
      var issuer = _configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "SmartHub";
      var audience = _configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "SmartHubClient";
      var expiryMinutes = int.TryParse(_configuration["Jwt:ExpireMinutes"], out var minutes) ? minutes : 60;

      var keyBytes = Encoding.UTF8.GetBytes(key);
      var signingKey = new SymmetricSecurityKey(keyBytes);
      var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

      var claims = new List<Claim>
      {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        new Claim(ClaimTypes.Role, user.Role.ToString())
      };

      var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
        signingCredentials: creds
      );

      var tokenHandler = new JwtSecurityTokenHandler();
      var tokenString = tokenHandler.WriteToken(token);

      var refreshToken = GenerateRefreshToken();
      var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

      return new AuthResponse
      {
        Token = tokenString,
        ExpiresAt = token.ValidTo,
        RefreshToken = refreshToken,
        RefreshTokenExpiry = refreshTokenExpiry,
        UserId = user.Id,
        Email = user.Email,
        FullName = $"{user.FirstName} {user.LastName}",
        Role = user.Role.ToString(),
        ProfileImageUrl = user.ProfileImagedUrl
      };
    }

    private string GenerateRefreshToken()
    {
      var randomNumber = RandomNumberGenerator.GetBytes(64);
      return Convert.ToBase64String(randomNumber);
    }
  }
}
