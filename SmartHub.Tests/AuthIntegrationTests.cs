using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using SmartHub.Api;
using SmartHub.Application.DTOs.Auth;
using SmartHub.Infrastructure.Persistence;
using Xunit;

namespace SmartHub.Tests
{
  public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
  {
    private readonly WebApplicationFactory<Program> _factory;

    public AuthIntegrationTests(WebApplicationFactory<Program> factory)
    {
      _factory = factory.WithWebHostBuilder(builder =>
      {
        builder.ConfigureServices(services =>
        {
          // replace SmartHubDbContext with in-memory test DB
          services.RemoveAll(typeof(DbContextOptions<SmartHubDbContext>));
          services.RemoveAll(typeof(SmartHubDbContext));
          services.AddDbContext<SmartHubDbContext>(options =>
            options.UseInMemoryDatabase("IntegrationTestDb"));
        });
        // Provide Jwt settings for integration test so tokens are generated
        builder.ConfigureAppConfiguration((context, config) =>
        {
          var dict = new System.Collections.Generic.Dictionary<string, string?>
          {
            { "Jwt:Key", "01234567890123456789012345678901" },
            { "Jwt:Issuer", "SmartHub" },
            { "Jwt:Audience", "SmartHubClient" },
            { "Jwt:ExpireMinutes", "60" }
          };
          config.AddInMemoryCollection(dict.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value!));
        });
      });
    }

    [Fact]
    public async Task FullAuthFlow_RegisterLoginRefreshLogout_Succeeds()
    {
      var client = _factory.CreateClient();
      var registerRequest = new RegisterRequest { FirstName = "Int", LastName = "Tester", Email = "inttest@example.com", Password = "IntPass!234", ConfirmPassword = "IntPass!234" };
      var registerResp = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
      registerResp.EnsureSuccessStatusCode();
      var registerContent = await registerResp.Content.ReadFromJsonAsync<AuthResponse>();

      var loginRequest = new LoginRequest { Email = registerRequest.Email, Password = registerRequest.Password };
      var loginResp = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
      loginResp.EnsureSuccessStatusCode();
      var loginContent = await loginResp.Content.ReadFromJsonAsync<AuthResponse>();

      var refreshReq = new RefreshTokenRequest { RefreshToken = loginContent.RefreshToken };
      var refreshResp = await client.PostAsJsonAsync("/api/auth/refresh", refreshReq);
      refreshResp.EnsureSuccessStatusCode();
      var refreshContent = await refreshResp.Content.ReadFromJsonAsync<AuthResponse>();

      // Logout (endpoint requires a valid access token)
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshContent.Token);
      var logoutResp = await client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest { RefreshToken = refreshContent.RefreshToken });
      if (logoutResp.StatusCode != System.Net.HttpStatusCode.NoContent)
      {
        var body = await logoutResp.Content.ReadAsStringAsync();
        Assert.True(false, $"Logout failed with {(int)logoutResp.StatusCode}: {body}");
      }

      // Trying to refresh with the same token should fail
      var refreshAgainResp = await client.PostAsJsonAsync("/api/auth/refresh", refreshReq);
      Assert.False(refreshAgainResp.IsSuccessStatusCode);
    }
  }
}
