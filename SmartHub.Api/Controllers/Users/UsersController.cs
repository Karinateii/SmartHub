using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHub.Infrastructure.Persistence;

namespace SmartHub.Api.Controllers.Users
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly SmartHubDbContext _db;
        public UsersController(SmartHubDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var users = await _db.Users.Select(u => new { u.Id, u.Email, Role = u.Role.ToString(), u.FirstName, u.LastName }).ToListAsync();
            return Ok(users);
        }
    }
}
