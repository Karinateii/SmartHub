using SmartHub.Application.DTOs.Auth;
using SmartHub.Application.Validators.Auth;
using Xunit;

namespace SmartHub.Tests
{
  public class AuthValidatorsTests
  {
    [Fact]
    public void LoginRequestValidator_ShouldFail_OnInvalidEmail()
    {
      var validator = new LoginRequestValidator();
      var request = new LoginRequest { Email = "", Password = "" };
      var result = validator.Validate(request);
      Assert.False(result.IsValid);
      Assert.Contains(result.Errors, e => e.PropertyName == "Email");
      Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void RegisterRequestValidator_ShouldFail_OnPasswordMismatch()
    {
      var validator = new RegisterRequestValidator();
      var request = new RegisterRequest
      {
        FirstName = "John",
        LastName = "Doe",
        Email = "john@example.com",
        Password = "123456",
        ConfirmPassword = "654321"
      };
      var result = validator.Validate(request);
      Assert.False(result.IsValid);
      Assert.Contains(result.Errors, e => e.PropertyName == "ConfirmPassword");
    }
  }
}
