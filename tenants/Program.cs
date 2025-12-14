using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Data;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.Services;
using LicenseManagement.Data.Middleware;
using LicenseManagement.Data.Results;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tenants API",
        Version = "v1",
        Description = "API for managing tenants"
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddLogging();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

var tenantGroup = app.MapGroup("/api/tenants")
    .WithName("Tenants");

tenantGroup.MapGet("/", GetAllTenants)
    .WithName("GetAllTenants")
    .WithSummary("Get all tenants")
    .Produces<ApiResult<List<Tenant>>>(StatusCodes.Status200OK);

tenantGroup.MapGet("/{tenantId}", GetTenantById)
    .WithName("GetTenantById")
    .WithSummary("Get tenant by ID")
    .Produces<ApiResult<Tenant>>(StatusCodes.Status200OK);

tenantGroup.MapPost("/", CreateTenant)
    .WithName("CreateTenant")
    .WithSummary("Create a new tenant")
    .Produces<ApiResult<Tenant>>(StatusCodes.Status201Created);

tenantGroup.MapPut("/{tenantId}", UpdateTenant)
    .WithName("UpdateTenant")
    .WithSummary("Update an existing tenant")
    .Produces<ApiResult<Tenant>>(StatusCodes.Status200OK);

tenantGroup.MapDelete("/{tenantId}", DeleteTenant)
    .WithName("DeleteTenant")
    .WithSummary("Delete a tenant")
    .Produces<ApiResponse>(StatusCodes.Status204NoContent);

app.Run();

async Task<IResult> GetAllTenants(ITenantService service)
{
    var tenants = await service.GetAllTenantsAsync();
    return Results.Ok(ApiResult<List<Tenant>>.SuccessResult(tenants, "Tenants retrieved successfully"));
}

async Task<IResult> GetTenantById(int tenantId, ITenantService service)
{
    var tenant = await service.GetTenantByIdAsync(tenantId);
    if (tenant == null)
        return Results.NotFound(ApiResult<Tenant>.FailureResult("Tenant not found"));
    return Results.Ok(ApiResult<Tenant>.SuccessResult(tenant, "Tenant retrieved successfully"));
}

async Task<IResult> CreateTenant(Tenant tenant, ITenantService service)
{
    var createdTenant = await service.CreateTenantAsync(tenant);
    return Results.CreatedAtRoute("GetTenantById", new { tenantId = createdTenant.TenantID },
        ApiResult<Tenant>.SuccessResult(createdTenant, "Tenant created successfully"));
}

async Task<IResult> UpdateTenant(int tenantId, Tenant tenantUpdate, ITenantService service)
{
    var tenant = await service.UpdateTenantAsync(tenantId, tenantUpdate);
    return Results.Ok(ApiResult<Tenant>.SuccessResult(tenant, "Tenant updated successfully"));
}

async Task<IResult> DeleteTenant(int tenantId, ITenantService service)
{
    await service.DeleteTenantAsync(tenantId);
    return Results.NoContent();
}
