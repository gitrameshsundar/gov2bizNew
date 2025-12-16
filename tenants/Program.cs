using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LicenseManagement.Data.Data;
using LicenseManagement.Data.Middleware;
using LicenseManagement.Data.Repositories;
using tenants.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// DEPENDENCY INJECTION CONFIGURATION
// ============================================================

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tenants API",
        Version = "v1",
        Description = "API for managing tenants (CQRS Pattern)"
    });
});

// Add Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Repository
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Add MediatR for CQRS pattern
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly, 
        typeof(LicenseManagement.Data.CQRS.Queries.GetAllTenantsQuery).Assembly));

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
// MIDDLEWARE CONFIGURATION
// ============================================================

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

app.MapTenantEndpoints();

app.Run();
