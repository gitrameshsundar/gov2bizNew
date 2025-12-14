using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LicenseManagement.Data.Middleware
{
    /// <summary>
    /// Middleware component for handling exceptions globally across the application.
    /// 
    /// DESIGN PATTERN: Middleware Pattern
    /// PURPOSE: Provides centralized exception handling and consistent error responses across all API endpoints.
    /// 
    /// BENEFITS:
    /// - Single point of exception handling
    /// - Consistent error response format
    /// - Prevents stack traces from leaking to clients in production
    /// - Centralized logging of all exceptions
    /// - Cleaner controller/endpoint code
    /// 
    /// USAGE: Register in Program.cs with app.UseMiddleware&lt;ExceptionHandlingMiddleware&gt;();
    /// 
    /// BEST PRACTICES IMPLEMENTED:
    /// 1. All exceptions are caught at application level
    /// 2. Sensitive error details are not exposed in production
    /// 3. Unique trace IDs for error tracking and debugging
    /// 4. Structured logging for error investigation
    /// 5. Appropriate HTTP status codes based on exception type
    /// 6. Consistent JSON response format
    /// 7. Thread-safe exception handling
    /// 8. Comprehensive XML documentation
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        /// <summary>
        /// Delegate representing the next middleware in the pipeline.
        /// This follows the standard ASP.NET Core middleware pattern.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Logger instance for recording exception details.
        /// Uses dependency injection to ensure single logger instance per middleware.
        /// </summary>
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// 
        /// Constructor Pattern: Constructor Injection
        /// - Dependencies are injected via constructor for better testability and loose coupling
        /// - Parameters are stored as readonly fields to prevent accidental modification
        /// </summary>
        /// <param name="next">The next middleware delegate in the request pipeline</param>
        /// <param name="logger">The logger instance for recording exception information</param>
        /// <exception cref="ArgumentNullException">Thrown when next or logger is null</exception>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            // Validate that critical dependencies are provided
            _next = next ?? throw new ArgumentNullException(nameof(next), "RequestDelegate cannot be null");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        }

        /// <summary>
        /// Invokes the middleware to process the HTTP request and handle any exceptions.
        /// 
        /// EXECUTION FLOW:
        /// 1. Wraps the next middleware in a try-catch block
        /// 2. If an exception occurs, logs the error details
        /// 3. Converts the exception to a standardized error response
        /// 4. Returns consistent JSON error response to the client
        /// 
        /// BEST PRACTICES:
        /// - Async all the way: Uses async/await for non-blocking operations
        /// - Exception transparency: All exceptions are caught and handled
        /// - Logging: Detailed logging for debugging and monitoring
        /// </summary>
        /// <param name="context">The HTTP context for the current request</param>
        /// <returns>A completed task representing the asynchronous operation</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Pass the request to the next middleware in the pipeline
                // If no exception occurs, the response is processed normally
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the exception with full context for troubleshooting
                // This ensures all errors are recorded regardless of where they occur
                _logger.LogError(ex, "An unhandled exception occurred while processing request {RequestPath}. " +
                    "TraceId: {TraceId}, Method: {Method}",
                    context.Request.Path,
                    context.TraceIdentifier,
                    context.Request.Method);

                // Handle the exception and generate consistent error response
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        /// <summary>
        /// Handles the exception and generates a standardized error response.
        /// 
        /// STRATEGY: Exception Type Mapping
        /// - Maps different exception types to appropriate HTTP status codes
        /// - Ensures consistent error response format regardless of exception type
        /// - Prevents sensitive stack traces from reaching clients
        /// 
        /// SUPPORTED EXCEPTION TYPES:
        /// - ArgumentException ? 400 Bad Request
        /// - KeyNotFoundException ? 404 Not Found
        /// - UnauthorizedAccessException ? 401 Unauthorized
        /// - DbUpdateException ? 409 Conflict
        /// - InvalidOperationException ? 400 Bad Request
        /// - Default (Any unhandled exception) ? 500 Internal Server Error
        /// 
        /// BEST PRACTICES:
        /// - Uses context parameter for logging
        /// - Pure exception handling: No side effects
        /// - Defensive coding: All properties are safely initialized
        /// - JSON serialization: Uses System.Text.Json for performance
        /// </summary>
        /// <param name="context">The HTTP context for the current request</param>
        /// <param name="exception">The exception that was thrown and needs handling</param>
        /// <param name="logger">Logger for recording exception details</param>
        /// <returns>A completed task representing the asynchronous response writing</returns>
        /// <remarks>
        /// HTTP STATUS CODES:
        /// 
        /// 400 Bad Request (ArgumentException, InvalidOperationException)
        /// - User input validation failed
        /// - Invalid parameters or malformed request data
        /// - Client error - request should be corrected before retry
        /// 
        /// 404 Not Found (KeyNotFoundException)
        /// - Requested resource does not exist
        /// - Resource ID is invalid or deleted
        /// - Client error - resource must exist before operation
        /// 
        /// 401 Unauthorized (UnauthorizedAccessException)
        /// - User is not authenticated or lacks permission
        /// - Authentication token is invalid or expired
        /// - Client error - user must authenticate before retry
        /// 
        /// 409 Conflict (DbUpdateException)
        /// - Database constraint violation
        /// - Unique constraint failed
        /// - Server error - data integrity issue
        /// 
        /// 500 Internal Server Error (Default/Unexpected)
        /// - Unexpected server-side error
        /// - Database connectivity issues
        /// - Configuration problems
        /// - Server error - client should retry later
        /// </remarks>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<ExceptionHandlingMiddleware> logger)
        {
            // Ensure response is JSON format for API consistency
            context.Response.ContentType = "application/json";

            // Initialize the error response with common properties
            var response = new ErrorResponse
            {
                Timestamp = DateTime.UtcNow,  // Record exact time of error occurrence
                TraceId = context.TraceIdentifier,  // Unique ID for correlating logs with HTTP request
                RequestPath = context.Request.Path.Value ?? "/",  // Path of the request that caused the error
                RequestMethod = context.Request.Method  // HTTP method of the failed request
            };

            // Map exception types to appropriate HTTP status codes
            // This pattern ensures consistent error handling across the application
            switch (exception)
            {
                // CASE 1: ArgumentException - Invalid input or parameters
                // HTTP 400: Client provided invalid data
                case ArgumentException argEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = argEx.Message;
                    response.ErrorType = exception.GetType().Name;
                    logger.LogWarning("Validation error occurred: {Message}", argEx.Message);
                    break;

                // CASE 2: KeyNotFoundException - Resource not found
                // HTTP 404: Requested resource does not exist
                case KeyNotFoundException knfEx:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = knfEx.Message;
                    response.ErrorType = exception.GetType().Name;
                    logger.LogWarning("Resource not found: {Message}", knfEx.Message);
                    break;

                // CASE 3: UnauthorizedAccessException - Authentication/Authorization failed
                // HTTP 401: User is not authenticated or lacks required permissions
                case UnauthorizedAccessException uaEx:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = "Access denied. Authentication required or insufficient permissions.";
                    response.ErrorType = exception.GetType().Name;
                    logger.LogWarning("Unauthorized access attempted: {Message}", uaEx.Message);
                    break;

                // CASE 4: DbUpdateException - Database operation failed
                // HTTP 409: Conflict - typically due to constraint violations
                case DbUpdateException dbEx:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    response.StatusCode = StatusCodes.Status409Conflict;
                    response.Message = "A database constraint violation occurred. Please verify your data and try again.";
                    response.ErrorType = "DatabaseError";
                    logger.LogError(dbEx, "Database update error occurred");
                    break;

                // CASE 5: InvalidOperationException - Invalid operation state
                // HTTP 400: Client cannot perform requested operation in current state
                case InvalidOperationException ioEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = ioEx.Message;
                    response.ErrorType = exception.GetType().Name;
                    logger.LogWarning("Invalid operation: {Message}", ioEx.Message);
                    break;

                // CASE 6: TimeoutException - Operation took too long
                // HTTP 408: Request Timeout - operation could not complete in time
                case TimeoutException texEx:
                    context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
                    response.StatusCode = StatusCodes.Status408RequestTimeout;
                    response.Message = "The request took too long to process. Please try again.";
                    response.ErrorType = exception.GetType().Name;
                    logger.LogWarning("Request timeout occurred: {Message}", texEx.Message);
                    break;

                // CASE 7: NotImplementedException - Feature not yet implemented
                // HTTP 501: Not Implemented - feature is not available
                case NotImplementedException nieEx:
                    context.Response.StatusCode = StatusCodes.Status501NotImplemented;
                    response.StatusCode = StatusCodes.Status501NotImplemented;
                    response.Message = "This feature is not yet implemented.";
                    response.ErrorType = exception.GetType().Name;
                    logger.LogWarning("Not implemented feature accessed: {Message}", nieEx.Message);
                    break;

                // DEFAULT CASE: Unexpected/Unhandled exception
                // HTTP 500: Internal Server Error - something went wrong on the server
                // This is the fallback for any exception type not explicitly handled above
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Message = "An unexpected error occurred while processing your request. Please contact support if the problem persists.";
                    response.ErrorType = exception.GetType().Name;
                    
                    // Log the full exception with stack trace for debugging
                    // This information should NOT be sent to the client
                    logger.LogError(exception, "Unhandled exception occurred. Type: {ExceptionType}",
                        exception.GetType().FullName);
                    break;
            }

            // Serialize the error response to JSON and write to response body
            // JsonSerializer.Serialize is used for performance benefits in .NET 5+
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false  // Compact JSON for production
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }

    /// <summary>
    /// Represents a standardized error response returned to clients.
    /// 
    /// DESIGN PATTERN: Data Transfer Object (DTO)
    /// PURPOSE: Provides a consistent structure for all error responses across the API.
    /// 
    /// PROPERTIES:
    /// - StatusCode: HTTP status code that was set
    /// - Message: User-friendly error message (safe for client display)
    /// - ErrorType: Type of exception that occurred (for client-side error handling)
    /// - Timestamp: Exact UTC time when the error occurred
    /// - TraceId: Unique identifier to correlate error with server logs
    /// - RequestPath: The API endpoint path that failed
    /// - RequestMethod: HTTP method (GET, POST, etc.) of the failed request
    /// 
    /// BEST PRACTICES:
    /// 1. All properties have default values to prevent null reference exceptions
    /// 2. Properties are initialized inline for defensive programming
    /// 3. XML documentation provides clear property descriptions
    /// 4. Immutable (no setters) would be more defensive, but mutable for flexibility
    /// 5. JSON property names match industry standards
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code of the error response.
        /// 
        /// Common values:
        /// - 400: Bad Request (client error in request format/data)
        /// - 401: Unauthorized (authentication required)
        /// - 404: Not Found (resource does not exist)
        /// - 408: Request Timeout (operation took too long)
        /// - 409: Conflict (constraint violation)
        /// - 500: Internal Server Error (unexpected server-side error)
        /// - 501: Not Implemented (feature not available)
        /// </summary>
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the user-friendly error message.
        /// 
        /// IMPORTANT: This message is sent to clients and should NOT contain:
        /// - Stack traces
        /// - Sensitive system information
        /// - Database details
        /// - Internal system paths
        /// - Database query information
        /// - Internal configuration details
        /// 
        /// Instead, use clear, actionable messages that help users understand
        /// what went wrong and how to fix it.
        /// 
        /// Example Good Message: "Username already exists. Please choose a different username."
        /// Example Bad Message: "Unique constraint 'UX_Username' violation in table 'Users'"
        /// 
        /// SECURITY NOTE: Always sanitize user-facing error messages to prevent
        /// information disclosure vulnerabilities.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of exception that occurred.
        /// 
        /// PURPOSE: Allows client-side code to handle specific exception types
        /// without parsing the message string.
        /// 
        /// EXAMPLE USAGE (Client-Side JavaScript):
        /// ```javascript
        /// if (error.errorType === "ArgumentException") {
        ///     // Handle validation error with visual indicator
        ///     showValidationError(error.message);
        /// } else if (error.errorType === "KeyNotFoundException") {
        ///     // Handle not found error with redirect
        ///     redirectTo('/dashboard');
        /// } else if (error.errorType === "DatabaseError") {
        ///     // Handle database error
        ///     showRetryOption();
        /// }
        /// ```
        /// 
        /// VALID VALUES:
        /// - ArgumentException
        /// - KeyNotFoundException
        /// - UnauthorizedAccessException
        /// - DatabaseError
        /// - InvalidOperationException
        /// - TimeoutException
        /// - NotImplementedException
        /// - [Unhandled exception class name]
        /// </summary>
        [JsonPropertyName("errorType")]
        public string? ErrorType { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when the error occurred.
        /// 
        /// FORMAT: ISO 8601 (e.g., "2024-12-14T10:30:45.1234567Z")
        /// TIMEZONE: Always UTC for consistency across systems and time zones
        /// 
        /// PURPOSE:
        /// - Track when errors occur for time-based analysis
        /// - Correlate with server logs and monitoring systems
        /// - Identify patterns or recurring issues
        /// - Support for error investigation and root cause analysis
        /// - Performance analysis (identify peak error times)
        /// 
        /// EXAMPLE USE CASES:
        /// - Filter logs by specific time range
        /// - Create incident timeline
        /// - Analyze error frequency patterns
        /// - Verify fix effectiveness before/after deployment
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the trace identifier for the request.
        /// 
        /// DEFINITION: A unique identifier automatically generated by ASP.NET Core
        /// for each HTTP request. Format: typically a 32-character hex string or
        /// formatted as "ParentId:ChildActivityId".
        /// 
        /// EXAMPLE FORMAT: "0HN4BIQUG2K5I:00000001"
        /// 
        /// PURPOSE:
        /// - Correlate client requests with server-side logs
        /// - Trace request flow through multiple services/logs
        /// - Essential for debugging in distributed systems and microservices
        /// - Helps support team quickly find logs related to specific error
        /// - Track request across multiple middleware/services
        /// 
        /// USAGE WORKFLOW (Support Team):
        /// 1. Customer reports issue and provides TraceId
        /// 2. Support team searches logs for this TraceId
        /// 3. Complete request flow is visible in logs
        /// 4. Root cause is identified quickly
        /// 5. Issue is resolved or escalated to development
        /// 
        /// DISTRIBUTED TRACING:
        /// In microservices architecture, the same TraceId flows through:
        /// - API Gateway
        /// - Multiple microservices
        /// - Database operations
        /// - External service calls
        /// 
        /// This enables end-to-end visibility across entire system.
        /// </summary>
        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the request path that caused the error.
        /// 
        /// FORMAT: URL path without query string
        /// - "/api/customers/123" 
        /// - "/api/users"
        /// - "/api/licenses"
        /// 
        /// Note: Query parameters are NOT included (see RequestPath property)
        /// Full URL with query would be: "/api/customers?skip=0&take=10"
        /// RequestPath only includes: "/api/customers"
        /// 
        /// PURPOSE:
        /// - Identify which endpoint caused the error
        /// - Track patterns of failures on specific endpoints
        /// - Support debugging and error investigation
        /// - Monitor endpoint-specific health metrics
        /// - Identify problematic API routes
        /// 
        /// ANALYSIS USE CASES:
        /// - "GET /api/licenses keeps failing" - database issue?
        /// - "POST /api/customers timing out" - performance issue?
        /// - "DELETE endpoints are unstable" - permission/constraint issue?
        /// </summary>
        [JsonPropertyName("requestPath")]
        public string RequestPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTTP method of the request that caused the error.
        /// 
        /// VALID VALUES (HTTP VERBS):
        /// - GET: Retrieve/read resource (idempotent, safe)
        /// - POST: Create new resource (not idempotent)
        /// - PUT: Replace entire resource (idempotent)
        /// - PATCH: Partial update to resource (may or may not be idempotent)
        /// - DELETE: Remove resource (idempotent)
        /// - HEAD: Like GET but without response body
        /// - OPTIONS: Get communication options available
        /// 
        /// IDEMPOTENT METHODS: GET, PUT, DELETE (safe to retry)
        /// NON-IDEMPOTENT METHODS: POST, PATCH (must be careful with retries)
        /// 
        /// PURPOSE:
        /// - Identify what operation failed
        /// - Pattern analysis (e.g., all DELETE requests failing)
        /// - Client-side error handling strategies
        /// - Determine retry safety
        /// - Understand operation semantics
        /// 
        /// PATTERN ANALYSIS EXAMPLES:
        /// - All GET requests failing ? Server connectivity issue
        /// - All POST requests failing ? Validation rules changed
        /// - All DELETE requests failing ? Permission issue
        /// - All PUT requests failing ? Concurrency/update logic issue
        /// 
        /// RETRY DECISION LOGIC:
        /// - If GET failed ? Safe to retry automatically
        /// - If POST failed ? Must show confirmation before retry
        /// - If DELETE failed ? Careful retry needed (may be partially deleted)
        /// </summary>
        [JsonPropertyName("requestMethod")]
        public string RequestMethod { get; set; } = string.Empty;
    }
}