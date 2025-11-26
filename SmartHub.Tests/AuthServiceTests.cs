using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartHub.Infrastructure.Persistence;
using SmartHub.Infrastructure.Services;
using SmartHub.Application.DTOs.Auth;
using SmartHub.Infrastructure.Persistence;
using SmartHub.Infrastructure.Services;
using Xunit;

namespace SmartHub.Tests
{
  public class AuthServiceTests
  {
    private SmartHubDbContext CreateInMemoryDb(string dbName)
    {
      var options = new DbContextOptionsBuilder<SmartHubDbContext>()
        .UseInMemoryDatabase(dbName)
        .Options;
      return new SmartHubDbContext(options);
    }

    private IConfiguration CreateJwtConfig()
    {
      // Generate a runtime JWT key for tests to avoid hardcoding secrets in source
      var rnd = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
      var runtimeKey = Convert.ToBase64String(rnd);
      var dict = new System.Collections.Generic.Dictionary<string, string>
      {
        { "Jwt:Key", runtimeKey },
        { "Jwt:Issuer", "SmartHub" },
        { "Jwt:Audience", "SmartHubClient" },
        { "Jwt:ExpireMinutes", "60" }
      };
      return new ConfigurationBuilder().AddInMemoryCollection(dict.Select(kvp => new System.Collections.Generic.KeyValuePair<string, string?>(kvp.Key, kvp.Value))).Build();
    }

    [Fact]
    public async Task Register_ShouldHashRefreshTokenAndStoreHashedValue()
    {
      using var db = CreateInMemoryDb("RegisterTestDb");
      var config = CreateJwtConfig();
      var service = new AuthService(db, config);
      var pwd = "Pass123!";
      var request = new RegisterRequest { FirstName = "T", LastName = "User", Email = "test@example.com", Password = pwd, ConfirmPassword = pwd };

      var response = await service.RegisterAsync(request);

      var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
      Assert.NotNull(user);
      Assert.NotNull(response.RefreshToken);
      Assert.NotNull(user.RefreshToken);
      Assert.NotEqual(response.RefreshToken, user.RefreshToken);
      Assert.True(BCrypt.Net.BCrypt.Verify(response.RefreshToken, user.RefreshToken));
    }

    [Fact]
    public async Task Login_ShouldVerifyPasswordAndStoreHashedRefreshToken()
    {
      using var db = CreateInMemoryDb("LoginTestDb");
      var config = CreateJwtConfig();
      var service = new AuthService(db, config);
      var loginPwd = "Login!234";
      var register = new RegisterRequest { FirstName = "L", LastName = "User", Email = "login@example.com", Password = loginPwd, ConfirmPassword = loginPwd };
      var regResponse = await service.RegisterAsync(register);

      var loginRequest = new LoginRequest { Email = register.Email, Password = register.Password };
      var loginResponse = await service.LoginAsync(loginRequest);
      var user = await db.Users.FirstOrDefaultAsync(u => u.Email == register.Email);

      Assert.NotNull(loginResponse.RefreshToken);
      Assert.NotNull(user.RefreshToken);
      Assert.True(BCrypt.Net.BCrypt.Verify(loginResponse.RefreshToken, user.RefreshToken));
    }

    [Fact]
    public async Task RefreshToken_ShouldFailForInvalidToken()
    {
      using var db = CreateInMemoryDb("RefreshInvalidTestDb");
      var config = CreateJwtConfig();
      var service = new AuthService(db, config);
      var refreshPwd = "Refresh!234";
      var register = new RegisterRequest { FirstName = "R", LastName = "User", Email = "refresh@example.com", Password = refreshPwd, ConfirmPassword = refreshPwd };
      var regResponse = await service.RegisterAsync(register);

      await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.RefreshTokenAsync("invalidtoken"));
    }

    [Fact]
    public async Task Logout_ShouldRevokeRefreshToken()
    {
      using var db = CreateInMemoryDb("LogoutTestDb");
      var config = CreateJwtConfig();
      var service = new AuthService(db, config);
      var logoutPwd = "Logout!234";
      var register = new RegisterRequest { FirstName = "O", LastName = "User", Email = "logout@example.com", Password = logoutPwd, ConfirmPassword = logoutPwd };
      var regResponse = await service.RegisterAsync(register);

      // ensure logout removes refresh token
      await service.RevokeRefreshTokenAsync(regResponse.RefreshToken);
      var user = await db.Users.FirstOrDefaultAsync(u => u.Email == register.Email);
      Assert.Null(user.RefreshToken);
      Assert.Null(user.RefreshTokenExpiry);
    }
  }
}
