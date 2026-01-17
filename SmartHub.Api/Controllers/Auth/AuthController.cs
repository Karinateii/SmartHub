using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using SmartHub.Application.DTOs.Auth;
using SmartHub.Application.Interfaces.Services;

namespace SmartHub.Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost("register")]
        [EnableRateLimiting("auth-register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }


        [HttpPost("login")]
        [EnableRateLimiting("auth-login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }


        [HttpPost("refresh")]
        [EnableRateLimiting("auth-refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }


        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            try
            {
                await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
            }
            catch (InvalidOperationException)
            {
                // Fallback: revoke by authenticated user id if token verification failed
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    await _authService.RevokeRefreshTokenByUserIdAsync(userId);
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
    }
}
