using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;
using LicenseManagement.Data.Services;

namespace tenants.Endpoints
{
    /// <summary>
    /// Endpoint handlers for Tenant API operations.
    /// 
    /// DESIGN PATTERN: Endpoint Organization Pattern (Minimal APIs)
    /// PURPOSE: Organizes all tenant-related HTTP endpoint handlers in a single class.
    /// 
    /// BENEFITS:
    /// - Keeps Program.cs clean and focused on configuration
    /// - Groups related endpoints together
    /// - Easy to test endpoint logic
    /// - Follows Single Responsibility Principle
    /// - Better code organization and maintainability
    /// - Scalable architecture
    /// 
    /// USAGE: In Program.cs:
    /// app.MapTenantEndpoints();
    /// </summary>
    public static class TenantEndpoints
    {
        /// <summary>
        /// Registers all tenant-related endpoints to the application.
        /// </summary>
        /// <param name="app">The WebApplication instance</param>
        /// <returns>The WebApplication for method chaining</returns>
        public static WebApplication MapTenantEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/tenants")
                .WithName("Tenants")
                .WithOpenApi();

            // GET /api/tenants
            group.MapGet("/", GetAllTenants)
                .WithName("GetAllTenants")
                .WithSummary("Get all tenants")
                .WithDescription("Retrieves a list of all tenants in the system")
                .Produces<ApiResult<List<Tenant>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // GET /api/tenants/{tenantId}
            group.MapGet("/{tenantId}", GetTenantById)
                .WithName("GetTenantById")
                .WithSummary("Get tenant by ID")
                .WithDescription("Retrieves a specific tenant by their ID")
                .Produces<ApiResult<Tenant>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            // POST /api/tenants
            group.MapPost("/", CreateTenant)
                .WithName("CreateTenant")
                .WithSummary("Create a new tenant")
                .WithDescription("Creates a new tenant record in the system")
                .Accepts<Tenant>("application/json")
                .Produces<ApiResult<Tenant>>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // PUT /api/tenants/{tenantId}
            group.MapPut("/{tenantId}", UpdateTenant)
                .WithName("UpdateTenant")
                .WithSummary("Update an existing tenant")
                .WithDescription("Updates tenant information by ID")
                .Accepts<Tenant>("application/json")
                .Produces<ApiResult<Tenant>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // DELETE /api/tenants/{tenantId}
            group.MapDelete("/{tenantId}", DeleteTenant)
                .WithName("DeleteTenant")
                .WithSummary("Delete a tenant")
                .WithDescription("Removes a tenant record from the system")
                .Produces<ApiResponse>(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            return app;
        }

        /// <summary>
        /// Handles GET /api/tenants - Retrieve all tenants.
        /// </summary>
        private static async Task<IResult> GetAllTenants(ITenantService service)
        {
            var tenants = await service.GetAllTenantsAsync();
            return Results.Ok(ApiResult<List<Tenant>>.SuccessResult(
                tenants,
                "Tenants retrieved successfully"));
        }

        /// <summary>
        /// Handles GET /api/tenants/{tenantId} - Retrieve a specific tenant.
        /// </summary>
        private static async Task<IResult> GetTenantById(int tenantId, ITenantService service)
        {
            if (tenantId <= 0)
                return Results.BadRequest(ApiResult<Tenant>.FailureResult(
                    "Invalid tenant ID. Must be greater than 0."));

            var tenant = await service.GetTenantByIdAsync(tenantId);

            if (tenant == null)
                return Results.NotFound(ApiResult<Tenant>.FailureResult(
                    $"Tenant with ID {tenantId} not found."));

            return Results.Ok(ApiResult<Tenant>.SuccessResult(
                tenant,
                "Tenant retrieved successfully"));
        }

        /// <summary>
        /// Handles POST /api/tenants - Create a new tenant.
        /// </summary>
        private static async Task<IResult> CreateTenant(Tenant tenant, ITenantService service)
        {
            var createdTenant = await service.CreateTenantAsync(tenant);

            return Results.CreatedAtRoute(
                "GetTenantById",
                new { tenantId = createdTenant.TenantID },
                ApiResult<Tenant>.SuccessResult(
                    createdTenant,
                    "Tenant created successfully"));
        }

        /// <summary>
        /// Handles PUT /api/tenants/{tenantId} - Update an existing tenant.
        /// </summary>
        private static async Task<IResult> UpdateTenant(int tenantId, Tenant tenantUpdate, ITenantService service)
        {
            if (tenantId <= 0)
                return Results.BadRequest(ApiResult<Tenant>.FailureResult(
                    "Invalid tenant ID. Must be greater than 0."));

            var updatedTenant = await service.UpdateTenantAsync(tenantId, tenantUpdate);

            return Results.Ok(ApiResult<Tenant>.SuccessResult(
                updatedTenant,
                "Tenant updated successfully"));
        }

        /// <summary>
        /// Handles DELETE /api/tenants/{tenantId} - Delete a tenant.
        /// </summary>
        private static async Task<IResult> DeleteTenant(int tenantId, ITenantService service)
        {
            if (tenantId <= 0)
                return Results.BadRequest(ApiResult<Tenant>.FailureResult(
                    "Invalid tenant ID. Must be greater than 0."));

            await service.DeleteTenantAsync(tenantId);

            return Results.NoContent();
        }
    }
}