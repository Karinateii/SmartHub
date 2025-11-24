using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartHub.Infrastructure.Persistence;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database connection
// Use the key defined in appsettings.json (`SmartHubDatabase`)
builder.Services.AddDbContext<SmartHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SmartHubDatabase")));


var app = builder.Build();
