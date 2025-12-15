using Microsoft.OpenApi;

namespace ApiGateway.Endpoints
{
    /// <summary>
    /// Gateway management and information endpoints.
    /// 
    /// DESIGN PATTERN: Endpoint Organization Pattern (Minimal APIs)
    /// PURPOSE: Provides gateway metadata, health checks, and service discovery
    /// 
    /// ENDPOINTS:
    /// - GET / - Root endpoint with gateway information
    /// - GET /health - Gateway health status
    /// - GET /gateway/info - Detailed gateway information
    /// 
    /// BENEFITS:
    /// - Keeps Program.cs clean and focused on configuration
    /// - Centralizes gateway information endpoints
    /// - Easy to test and maintain
    /// - Single Responsibility Principle
    /// - Better code organization
    /// </summary>
    public static class GatewayEndpoints
    {
        /// <summary>
        /// Registers all gateway management endpoints to the application.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder</param>
        public static void MapGatewayEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // GET /
            endpoints.MapGet("/", RootHandler)
                .WithName("Root")
                .WithSummary("API Gateway root endpoint")
                .AllowAnonymous()
                .Produces<RootResponse>(StatusCodes.Status200OK)
                .WithMetadata(new Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute());

            // GET /health
            endpoints.MapGet("/health", HealthHandler)
                .WithName("HealthCheck")
                .WithSummary("API Gateway health check")
                .AllowAnonymous()
                .Produces<HealthResponse>(StatusCodes.Status200OK)
                .WithMetadata(new Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute());

            // GET /gateway/info
            endpoints.MapGet("/gateway/info", GatewayInfoHandler)
                .WithName("GatewayInfo")
                .WithSummary("Get API Gateway information")
                .AllowAnonymous()
                .Produces<GatewayInfoResponse>(StatusCodes.Status200OK)
                .WithMetadata(new Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute());
        }

        /// <summary>
        /// Handles GET / - Root endpoint with gateway information.
        /// 
        /// HTTP METHOD: GET (Safe, Idempotent)
        /// RESPONSE CODE: 200 OK
        /// 
        /// PURPOSE: Entry point for API consumers
        /// Returns useful links for API documentation and health checks
        /// </summary>
        /// <returns>Gateway root information</returns>
        private static async Task<IResult> RootHandler()
        {
            return Results.Ok(new RootResponse
            {
                Message = "API Gateway",
                Version = "1.0",
                SwaggerUrl = "/swagger/index.html"
            });
        }

        /// <summary>
        /// Handles GET /health - Gateway health status check.
        /// 
        /// HTTP METHOD: GET (Safe, Idempotent)
        /// RESPONSE CODE: 200 OK
        /// 
        /// PURPOSE: Monitor gateway health
        /// Used by load balancers, monitoring systems, and health checks
        /// </summary>
        /// <returns>Health status</returns>
        private static async Task<IResult> HealthHandler()
        {
            return Results.Ok(new HealthResponse
            {
                Status = "healthy",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Handles GET /gateway/info - Detailed gateway information.
        /// 
        /// HTTP METHOD: GET (Safe, Idempotent)
        /// RESPONSE CODE: 200 OK
        /// 
        /// PURPOSE: Provide gateway configuration and service information
        /// Used for admin dashboards and system monitoring
        /// </summary>
        /// <returns>Gateway information</returns>
        private static async Task<IResult> GatewayInfoHandler()
        {
            return Results.Ok(new GatewayInfoResponse
            {
                Name = "API Gateway",
                Version = "1.0",
                Services = new[]
                {
                    new ServiceInfo { Name = "Customers Service", Url = "https://localhost:7084" },
                    new ServiceInfo { Name = "Users Service", Url = "https://localhost:7001" },
                    new ServiceInfo { Name = "Tenants Service", Url = "https://localhost:7002" },
                    new ServiceInfo { Name = "Licenses Service", Url = "https://localhost:7003" },
                    new ServiceInfo { Name = "Notifications Service", Url = "https://localhost:7004" }
                }
            });
        }
    }

    // ============================================================
    // RESPONSE MODELS
    // ============================================================

    /// <summary>
    /// Root endpoint response.
    /// </summary>
    public class RootResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string SwaggerUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Health check response.
    /// </summary>
    public class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Gateway information response.
    /// </summary>
    public class GatewayInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public ServiceInfo[] Services { get; set; } = Array.Empty<ServiceInfo>();
    }

    /// <summary>
    /// Service information in gateway info response.
    /// </summary>
    public class ServiceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}