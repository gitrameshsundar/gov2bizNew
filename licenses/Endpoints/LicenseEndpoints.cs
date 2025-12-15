using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;
using LicenseManagement.Data.Services;

namespace licenses.Endpoints
{
    /// <summary>
    /// Endpoint handlers for License API operations.
    /// 
    /// DESIGN PATTERN: Endpoint Organization Pattern (Minimal APIs)
    /// PURPOSE: Organizes all license-related HTTP endpoint handlers in a single class.
    /// </summary>
    public static class LicenseEndpoints
    {
        /// <summary>
        /// Registers all license-related endpoints to the application.
        /// </summary>
        /// <param name="app">The WebApplication instance</param>
        /// <returns>The WebApplication for method chaining</returns>
        public static WebApplication MapLicenseEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/licenses")
                .WithName("Licenses")
                .WithOpenApi();

            // GET /api/licenses
            group.MapGet("/", GetAllLicenses)
                .WithName("GetAllLicenses")
                .WithSummary("Get all licenses")
                .WithDescription("Retrieves a list of all licenses in the system")
                .Produces<ApiResult<List<License>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // GET /api/licenses/{licenseId}
            group.MapGet("/{licenseId}", GetLicenseById)
                .WithName("GetLicenseById")
                .WithSummary("Get license by ID")
                .WithDescription("Retrieves a specific license by their ID")
                .Produces<ApiResult<License>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            // POST /api/licenses
            group.MapPost("/", CreateLicense)
                .WithName("CreateLicense")
                .WithSummary("Create a new license")
                .WithDescription("Creates a new license record in the system")
                .Accepts<License>("application/json")
                .Produces<ApiResult<License>>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // PUT /api/licenses/{licenseId}
            group.MapPut("/{licenseId}", UpdateLicense)
                .WithName("UpdateLicense")
                .WithSummary("Update an existing license")
                .WithDescription("Updates license information by ID")
                .Accepts<License>("application/json")
                .Produces<ApiResult<License>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // DELETE /api/licenses/{licenseId}
            group.MapDelete("/{licenseId}", DeleteLicense)
                .WithName("DeleteLicense")
                .WithSummary("Delete a license")
                .WithDescription("Removes a license record from the system")
                .Produces<ApiResponse>(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            return app;
        }

        /// <summary>
        /// Handles GET /api/licenses - Retrieve all licenses.
        /// </summary>
        private static async Task<IResult> GetAllLicenses(ILicenseService service)
        {
            var licenses = await service.GetAllLicensesAsync();
            return Results.Ok(ApiResult<List<License>>.SuccessResult(
                licenses,
                "Licenses retrieved successfully"));
        }

        /// <summary>
        /// Handles GET /api/licenses/{licenseId} - Retrieve a specific license.
        /// </summary>
        private static async Task<IResult> GetLicenseById(int licenseId, ILicenseService service)
        {
            if (licenseId <= 0)
                return Results.BadRequest(ApiResult<License>.FailureResult(
                    "Invalid license ID. Must be greater than 0."));

            var license = await service.GetLicenseByIdAsync(licenseId);

            if (license == null)
                return Results.NotFound(ApiResult<License>.FailureResult(
                    $"License with ID {licenseId} not found."));

            return Results.Ok(ApiResult<License>.SuccessResult(
                license,
                "License retrieved successfully"));
        }

        /// <summary>
        /// Handles POST /api/licenses - Create a new license.
        /// </summary>
        private static async Task<IResult> CreateLicense(License license, ILicenseService service)
        {
            var createdLicense = await service.CreateLicenseAsync(license);

            return Results.CreatedAtRoute(
                "GetLicenseById",
                new { licenseId = createdLicense.LicenseID },
                ApiResult<License>.SuccessResult(
                    createdLicense,
                    "License created successfully"));
        }

        /// <summary>
        /// Handles PUT /api/licenses/{licenseId} - Update an existing license.
        /// </summary>
        private static async Task<IResult> UpdateLicense(int licenseId, License licenseUpdate, ILicenseService service)
        {
            if (licenseId <= 0)
                return Results.BadRequest(ApiResult<License>.FailureResult(
                    "Invalid license ID. Must be greater than 0."));

            var updatedLicense = await service.UpdateLicenseAsync(licenseId, licenseUpdate);

            return Results.Ok(ApiResult<License>.SuccessResult(
                updatedLicense,
                "License updated successfully"));
        }

        /// <summary>
        /// Handles DELETE /api/licenses/{licenseId} - Delete a license.
        /// </summary>
        private static async Task<IResult> DeleteLicense(int licenseId, ILicenseService service)
        {
            if (licenseId <= 0)
                return Results.BadRequest(ApiResult<License>.FailureResult(
                    "Invalid license ID. Must be greater than 0."));

            await service.DeleteLicenseAsync(licenseId);

            return Results.NoContent();
        }
    }
}