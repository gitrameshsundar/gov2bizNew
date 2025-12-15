using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LicenseManagement.Data.Data;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.Services;
using LicenseManagement.Data.Middleware;
using payments.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// DEPENDENCY INJECTION CONFIGURATION
// ============================================================

/// <summary>
/// Configures dependency injection for the payment service.
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

// Add API documentation (Swagger/OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payments API",
        Version = "v1",
        Description = "API for managing payment transactions",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "support@example.com"
        }
    });
});

// Add Database Context with SQL Server provider
// SCOPED LIFETIME: New DbContext instance per HTTP request
// This ensures proper resource cleanup and avoids thread-safety issues
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Repository and Service
// PATTERN: Dependency Injection with Scoped lifetime
// - Repository: Manages data access
// - Service: Manages business logic
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Add logging
builder.Services.AddLogging();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();

// ============================================================
// MIDDLEWARE CONFIGURATION (Pipeline)
// ============================================================

// Add exception handling middleware - must be first!
// This catches all exceptions thrown in downstream middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Add Swagger UI in development only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments API v1");
    });
}

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAll");

// ============================================================
// ENDPOINT MAPPING
// ============================================================

// Map all payment endpoints from the endpoint handler class
// This keeps Program.cs clean and organized
app.MapPaymentEndpoints();

// ============================================================
// START APPLICATION
// ============================================================

app.Run();