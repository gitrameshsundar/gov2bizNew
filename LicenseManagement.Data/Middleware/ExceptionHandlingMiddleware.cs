using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace LicenseManagement.Data.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred: {ex.Message}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Timestamp = DateTime.UtcNow,
                TraceId = context.TraceIdentifier
            };

            switch (exception)
            {
                case ArgumentException argEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.StatusCode = 400;
                    response.Message = argEx.Message;
                    break;

                case KeyNotFoundException knfEx:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response.StatusCode = 404;
                    response.Message = knfEx.Message;
                    break;

                case UnauthorizedAccessException uaEx:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.StatusCode = 401;
                    response.Message = uaEx.Message;
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.StatusCode = 500;
                    response.Message = "An unexpected error occurred";
                    break;
            }

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string TraceId { get; set; } = string.Empty;
    }
}