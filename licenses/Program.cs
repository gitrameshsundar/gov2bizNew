using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Licenses API",
        Version = "v1",
        Description = "API for managing licenses"
    });

    
});

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LicenseDbContext>(options =>
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

// License Endpoints
var licenseGroup = app.MapGroup("/api/licenses")
    .WithName("Licenses");
    //.RequireAuthorization();

licenseGroup.MapGet("/", GetAllLicenses)
    .WithName("GetAllLicenses")
    .WithSummary("Get all licenses")
    .Produces<List<License>>(StatusCodes.Status200OK);

licenseGroup.MapGet("/{licenseId}", GetLicenseById)
    .WithName("GetLicenseById")
    .WithSummary("Get license by ID")
    .Produces<License>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

licenseGroup.MapPost("/", CreateLicense)
    .WithName("CreateLicense")
    .WithSummary("Create a new license")
    .Accepts<License>("application/json")
    .Produces<License>(StatusCodes.Status201Created);

licenseGroup.MapPut("/{licenseId}", UpdateLicense)
    .WithName("UpdateLicense")
    .WithSummary("Update an existing license")
    .Accepts<License>("application/json")
    .Produces<License>(StatusCodes.Status200OK);

licenseGroup.MapDelete("/{licenseId}", DeleteLicense)
    .WithName("DeleteLicense")
    .WithSummary("Delete a license")
    .Produces(StatusCodes.Status204NoContent);

app.Run();

// Handlers
async Task<IResult> GetAllLicenses(LicenseDbContext db)
{
    var licenses = await db.Licenses.ToListAsync();
    return Results.Ok(licenses);
}

async Task<IResult> GetLicenseById(int licenseId, LicenseDbContext db)
{
    var license = await db.Licenses.FirstOrDefaultAsync(l => l.LicenseID == licenseId);
    if (license == null)
        return Results.NotFound(new { message = "License not found" });
    return Results.Ok(license);
}

async Task<IResult> CreateLicense(License license, LicenseDbContext db)
{
    if (string.IsNullOrWhiteSpace(license.Name))
        return Results.BadRequest(new { message = "License name is required" });

    db.Licenses.Add(license);
    await db.SaveChangesAsync();
    return Results.CreatedAtRoute("GetLicenseById", new { licenseId = license.LicenseID }, license);
}

async Task<IResult> UpdateLicense(int licenseId, License licenseUpdate, LicenseDbContext db)
{
    var license = await db.Licenses.FirstOrDefaultAsync(l => l.LicenseID == licenseId);
    if (license == null)
        return Results.NotFound(new { message = "License not found" });

    license.Name = licenseUpdate.Name;
    await db.SaveChangesAsync();
    return Results.Ok(license);
}

async Task<IResult> DeleteLicense(int licenseId, LicenseDbContext db)
{
    var license = await db.Licenses.FirstOrDefaultAsync(l => l.LicenseID == licenseId);
    if (license == null)
        return Results.NotFound(new { message = "License not found" });

    db.Licenses.Remove(license);
    await db.SaveChangesAsync();
    return Results.NoContent();
}

// DbContext
class LicenseDbContext : DbContext
{
    public LicenseDbContext(DbContextOptions<LicenseDbContext> options) : base(options) { }
    public DbSet<License> Licenses { get; set; } = null!;

}

// Models
class License
{
    public int LicenseID { get; set; }
    public string Name { get; set; } = string.Empty;
}
