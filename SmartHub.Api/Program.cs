using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SmartHub.Application.Interfaces.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using SmartHub.Application.Validators.Auth;
using SmartHub.Infrastructure.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database connection
// Use the key defined in appsettings.json (`SmartHubDatabase`)
builder.Services.AddDbContext<SmartHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SmartHubDatabase")));

// Register FluentValidation validators from Application assembly
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Register application services (AuthService in Infrastructure)
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT authentication if Jwt settings exist
var jwtKey = builder.Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");
if (!string.IsNullOrEmpty(jwtKey))
{
    var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER"),
                ValidAudience = builder.Configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
            };
        });
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Admin user from environment variables (for dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmartHubDbContext>();
    // For relational providers, run migrations; for in-memory (tests), ensure DB is created
    if (db.Database.IsRelational())
        db.Database.Migrate();
    else
        db.Database.EnsureCreated();
    var adminEmail = builder.Configuration["Admin:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL");
    var adminPassword = builder.Configuration["Admin:Password"] ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
    if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword) && !db.Users.Any(u => u.Email == adminEmail))
    {
        var adminUser = new SmartHub.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "Account",
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = SmartHub.Domain.Enums.Role.Admin,
            EmailVerified = true
        };
        db.Users.Add(adminUser);
        db.SaveChanges();
    }
}

app.Run();

// Allow WebApplicationFactory to reference Program class in integration tests
public partial class Program { }
