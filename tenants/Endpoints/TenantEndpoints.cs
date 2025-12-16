using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using LicenseManagement.Data.CQRS.Commands;
using LicenseManagement.Data.CQRS.Queries;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;

namespace tenants.Endpoints
{
    /// <summary>
    /// Endpoint handlers for Tenant API operations.
    /// 
    /// DESIGN PATTERN: CQRS (Command Query Responsibility Segregation) + Minimal APIs
    /// PURPOSE: Separates read (Query) and write (Command) operations for better scalability and maintainability.
    /// 
    /// CQRS BENEFITS:
    /// - Commands handle state modifications (Create, Update, Delete)
    /// - Queries handle data retrieval (Read)
    /// - Independent optimization of read/write paths
    /// - Better testability with isolated handlers
    /// - Improved scalability and flexibility
    /// </summary>
    public static class TenantEndpoints
    {
        /// <summary>
        /// Registers all tenant-related endpoints to the application.
        /// </summary>
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
                .Accepts<CreateTenantCommand>("application/json")
                .Produces<ApiResult<Tenant>>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // PUT /api/tenants/{tenantId}
            group.MapPut("/{tenantId}", UpdateTenant)
                .WithName("UpdateTenant")
                .WithSummary("Update an existing tenant")
                .WithDescription("Updates tenant information by ID")
                .Accepts<UpdateTenantCommand>("application/json")
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

        // ============================================================
        // QUERY ENDPOINTS (READ OPERATIONS)
        // ============================================================

        /// <summary>
        /// Handles GET /api/tenants - Retrieve all tenants.
        /// CQRS: Query Handler
        /// </summary>
        private static async Task<IResult> GetAllTenants(IMediator mediator)
        {
            var result = await mediator.Send(new GetAllTenantsQuery());
            return Results.Ok(result);
        }

        /// <summary>
        /// Handles GET /api/tenants/{tenantId} - Retrieve a specific tenant.
        /// CQRS: Query Handler
        /// </summary>
        private static async Task<IResult> GetTenantById(int tenantId, IMediator mediator)
        {
            if (tenantId <= 0)
                return Results.BadRequest(ApiResult<Tenant>.FailureResult(
                    "Invalid tenant ID. Must be greater than 0."));

            var result = await mediator.Send(new GetTenantByIdQuery(tenantId));
            
            if (!result.Success)
                return Results.NotFound(result);

            return Results.Ok(result);
        }

        // ============================================================
        // COMMAND ENDPOINTS (WRITE OPERATIONS)
        // ============================================================

        /// <summary>
        /// Handles POST /api/tenants - Create a new tenant.
        /// CQRS: Command Handler
        /// </summary>
        private static async Task<IResult> CreateTenant(CreateTenantCommand command, IMediator mediator)
        {
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.CreatedAtRoute(
                "GetTenantById",
                new { tenantId = result.Data?.TenantID },
                result);
        }

        /// <summary>
        /// Handles PUT /api/tenants/{tenantId} - Update an existing tenant.
        /// CQRS: Command Handler
        /// </summary>
        private static async Task<IResult> UpdateTenant(int tenantId, UpdateTenantCommand command, IMediator mediator)
        {
            if (tenantId <= 0)
                return Results.BadRequest(ApiResult<Tenant>.FailureResult(
                    "Invalid tenant ID. Must be greater than 0."));

            command.TenantId = tenantId;
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Ok(result);
        }

        /// <summary>
        /// Handles DELETE /api/tenants/{tenantId} - Delete a tenant.
        /// CQRS: Command Handler
        /// </summary>
        private static async Task<IResult> DeleteTenant(int tenantId, IMediator mediator)
        {
            if (tenantId <= 0)
                return Results.BadRequest(ApiResult<Tenant>.FailureResult(
                    "Invalid tenant ID. Must be greater than 0."));

            var result = await mediator.Send(new DeleteTenantCommand(tenantId));

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.NoContent();
        }
    }
}