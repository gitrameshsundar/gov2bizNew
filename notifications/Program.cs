using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LicenseManagement.Data.Data;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.Services;
using LicenseManagement.Data.Middleware;
using notifications.Endpoints;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notifications API",
        Version = "v1",
        Description = "API for managing notifications"
    });
});
// Add Database Context with SQL Server provider
// SCOPED LIFETIME: New DbContext instance per HTTP request
// This ensures proper resource cleanup and avoids thread-safety issues 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(connectionString));
// Add Repository and Service
// PATTERN: Dependency Injection with Scoped lifetime
// - Repository: Manages data access
// - Service: Manages business logic
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddLogging();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();  
    });
});

var app = builder.Build();
// ============================================================
// MIDDLEWARE CONFIGURATION (Pipeline)
// ============================================================

// Add exception handling middleware - must be first!
// This catches all exceptions thrown in downstream middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

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
app.MapNotificationEndpoints();

app.Run();

