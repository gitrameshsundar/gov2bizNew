using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LicenseManagement.Data.Data;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.Services;
using users.Endpoints;

var builder = WebApplication.CreateBuilder(args);
// ============================================================
// DEPENDENCY INJECTION CONFIGURATION
// ============================================================

/// <summary>
/// Configures dependency injection for the application.
/// 
/// DEPENDENCY INJECTION PATTERN:
/// - Constructor Injection: Dependencies passed via constructor
/// - Service Locator Pattern: Not used (anti-pattern in modern .NET)
/// 
/// LIFETIME MANAGEMENT:
/// - Singleton: Single instance for application lifetime
/// - Scoped: New instance per HTTP request (RECOMMENDED for DbContext)
/// - Transient: New instance every time (avoid for expensive objects)
/// 
/// CURRENT CONFIGURATION:
/// - DbContext: Scoped (one per request)
/// - Repository: Scoped (manages data access)
/// - Service: Scoped (manages business logic)
/// </summary>

// Add services
builder.Services.AddEndpointsApiExplorer();
// Add API documentation (Swagger/OpenAPI)
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Users API",
        Version = "v1",
        Description = "API for managing users and tenant assignments"
    });
});
// Add Database Context with SQL Server provider
// SCOPED LIFETIME: New DbContext instance per HTTP request
// This ensures proper resource cleanup and avoids thread-safety issues 
// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Repository and Service
// PATTERN: Dependency Injection with Scoped lifetime
// - Repository: Manages data access
// - Service: Manages business logic
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Add logging
builder.Services.AddLogging();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
// ============================================================
// ENDPOINT MAPPING
// ============================================================

// Map all customer endpoints from the endpoint handler class
// This keeps Program.cs clean and organized

app.MapUserEndpoints();

app.Run();




