using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
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

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/smarthub.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});
builder.Services.AddHealthChecks();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    // Global: 100 requests per 15 minutes per IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    // Stricter policy for Auth endpoints
    options.AddPolicy("auth-login", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });
    options.AddPolicy("auth-register", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromHours(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });
    options.AddPolicy("auth-refresh", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    // Log rejections and return 429 with details
    options.OnRejected = async (context, cancellationToken) =>
    {
        Log.Warning("Rate limit exceeded: Path={Path}, IP={IP}", context.HttpContext.Request.Path, context.HttpContext.Connection.RemoteIpAddress?.ToString());
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync("{\"error\":\"Too many requests\"}");
    };
});

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

// Fail fast if running in non-development without JWT configured
if (string.IsNullOrEmpty(jwtKey) && !builder.Environment.IsDevelopment())
{
    // Running in a non-development environment without JWT key is insecure; fail fast
    throw new InvalidOperationException("Missing JWT_KEY environment variable. Set JWT_KEY for authentication in production.");
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
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");

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


try
{
    Log.Information("Starting SmartHub API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SmartHub API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Allow WebApplicationFactory to reference Program class in integration tests
public partial class Program { }
