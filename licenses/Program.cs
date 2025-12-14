using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Data;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.Services;
using LicenseManagement.Data.Middleware;
using LicenseManagement.Data.Results;

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

builder.Services.AddScoped<ILicenseRepository, LicenseRepository>();
builder.Services.AddScoped<ILicenseService, LicenseService>();
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

// License Endpoints
var licenseGroup = app.MapGroup("/api/licenses")
    .WithName("Licenses");

licenseGroup.MapGet("/", GetAllLicenses)
    .WithName("GetAllLicenses")
    .WithSummary("Get all licenses")
    .Produces<ApiResult<List<License>>>(StatusCodes.Status200OK);

licenseGroup.MapGet("/{licenseId}", GetLicenseById)
    .WithName("GetLicenseById")
    .WithSummary("Get license by ID")
    .Produces<ApiResult<License>>(StatusCodes.Status200OK)
    .Produces<ApiResult<License>>(StatusCodes.Status404NotFound);

licenseGroup.MapPost("/", CreateLicense)
    .WithName("CreateLicense")
    .WithSummary("Create a new license")
    .Accepts<License>("application/json")
    .Produces<ApiResult<License>>(StatusCodes.Status201Created);

licenseGroup.MapPut("/{licenseId}", UpdateLicense)
    .WithName("UpdateLicense")
    .WithSummary("Update an existing license")
    .Accepts<License>("application/json")
    .Produces<ApiResult<License>>(StatusCodes.Status200OK);

licenseGroup.MapDelete("/{licenseId}", DeleteLicense)
    .WithName("DeleteLicense")
    .WithSummary("Delete a license")
    .Produces<ApiResponse>(StatusCodes.Status204NoContent);

app.Run();

// Handlers
async Task<IResult> GetAllLicenses(ILicenseService service)
{
    var licenses = await service.GetAllLicensesAsync();
    return Results.Ok(ApiResult<List<License>>.SuccessResult(licenses, "Licenses retrieved successfully"));
}

async Task<IResult> GetLicenseById(int licenseId, ILicenseService service)
{
    var license = await service.GetLicenseByIdAsync(licenseId);
    if (license == null)
        return Results.NotFound(ApiResult<License>.FailureResult("License not found"));
    return Results.Ok(ApiResult<License>.SuccessResult(license, "License retrieved successfully"));
}

async Task<IResult> CreateLicense(License license, ILicenseService service)
{
    var createdLicense = await service.CreateLicenseAsync(license);
    return Results.CreatedAtRoute("GetLicenseById", new { licenseId = createdLicense.LicenseID },
        ApiResult<License>.SuccessResult(createdLicense, "License created successfully"));
}

async Task<IResult> UpdateLicense(int licenseId, License licenseUpdate, ILicenseService service)
{
    var license = await service.UpdateLicenseAsync(licenseId, licenseUpdate);
    return Results.Ok(ApiResult<License>.SuccessResult(license, "License updated successfully"));
}

async Task<IResult> DeleteLicense(int licenseId, ILicenseService service)
{
    await service.DeleteLicenseAsync(licenseId);
    return Results.NoContent();
}

