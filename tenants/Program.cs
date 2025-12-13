using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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


var tenantGroup = app.MapGroup("/api/tenants")
    .WithName("Tenants");
    //.RequireAuthorization();

tenantGroup.MapGet("/", GetAllTenants)
    .WithName("GetAllTenants")
    .WithSummary("Get all tenants")
    .Produces<List<Tenant>>(StatusCodes.Status200OK);

tenantGroup.MapGet("/{tenantId}", GetTenantById)
    .WithName("GetTenantById")
    .WithSummary("Get tenant by ID")
    .Produces<Tenant>(StatusCodes.Status200OK);

tenantGroup.MapPost("/", CreateTenant)
    .WithName("CreateTenant")
    .WithSummary("Create a new tenant")
    .Produces<Tenant>(StatusCodes.Status201Created);

tenantGroup.MapPut("/{tenantId}", UpdateTenant)
    .WithName("UpdateTenant")
    .WithSummary("Update an existing tenant")
    .Produces<Tenant>(StatusCodes.Status200OK);

tenantGroup.MapDelete("/{tenantId}", DeleteTenant)
    .WithName("DeleteTenant")
    .WithSummary("Delete a tenant")
    .Produces(StatusCodes.Status204NoContent);

app.Run();

async Task<IResult> GetAllTenants(TenantDbContext db)
{
    var tenants = await db.Tenants.ToListAsync();
    return Results.Ok(tenants);
}

async Task<IResult> GetTenantById(int tenantId, TenantDbContext db)
{
    var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.TenantID == tenantId);
    if (tenant == null)
        return Results.NotFound(new { message = "Tenant not found" });
    return Results.Ok(tenant);
}

async Task<IResult> CreateTenant(Tenant tenant, TenantDbContext db)
{
    if (string.IsNullOrWhiteSpace(tenant.Name))
        return Results.BadRequest(new { message = "Tenant name is required" });

    db.Tenants.Add(tenant);
    await db.SaveChangesAsync();
    return Results.CreatedAtRoute("GetTenantById", new { tenantId = tenant.TenantID }, tenant);
}

async Task<IResult> UpdateTenant(int tenantId, Tenant tenantUpdate, TenantDbContext db)
{
    var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.TenantID == tenantId);
    if (tenant == null)
        return Results.NotFound(new { message = "Tenant not found" });

    tenant.Name = tenantUpdate.Name;
    await db.SaveChangesAsync();
    return Results.Ok(tenant);
}

async Task<IResult> DeleteTenant(int tenantId, TenantDbContext db)
{
    var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.TenantID == tenantId);
    if (tenant == null)
        return Results.NotFound(new { message = "Tenant not found" });

    db.Tenants.Remove(tenant);
    await db.SaveChangesAsync();
    return Results.NoContent();
}

class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }
    public DbSet<Tenant> Tenants { get; set; } = null!;
}

class Tenant
{
    public int TenantID { get; set; }
    public string Name { get; set; } = string.Empty;
}
