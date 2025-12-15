using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;
using LicenseManagement.Data.Services;

namespace notifications.Endpoints
{
    /// <summary>
    /// Endpoint handlers for Notification API operations.
    /// 
    /// DESIGN PATTERN: Endpoint Organization Pattern (Minimal APIs)
    /// PURPOSE: Organizes all notification-related HTTP endpoint handlers in a single class.
    /// </summary>
    public static class NotificationEndpoints
    {
        /// <summary>
        /// Registers all notification-related endpoints to the application.
        /// </summary>
        /// <param name="app">The WebApplication instance</param>
        /// <returns>The WebApplication for method chaining</returns>
        public static WebApplication MapNotificationEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/notifications")
                .WithName("Notifications")
                .WithOpenApi();

            // GET /api/notifications
            group.MapGet("/", GetAllNotifications)
                .WithName("GetAllNotifications")
                .WithSummary("Get all notifications")
                .WithDescription("Retrieves a list of all notifications in the system")
                .Produces<ApiResult<List<Notification>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // GET /api/notifications/{notificationId}
            group.MapGet("/{notificationId}", GetNotificationById)
                .WithName("GetNotificationById")
                .WithSummary("Get notification by ID")
                .WithDescription("Retrieves a specific notification by their ID")
                .Produces<ApiResult<Notification>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            // POST /api/notifications
            group.MapPost("/", CreateNotification)
                .WithName("CreateNotification")
                .WithSummary("Create a new notification")
                .WithDescription("Creates a new notification record in the system")
                .Accepts<Notification>("application/json")
                .Produces<ApiResult<Notification>>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // PUT /api/notifications/{notificationId}
            group.MapPut("/{notificationId}", UpdateNotification)
                .WithName("UpdateNotification")
                .WithSummary("Update an existing notification")
                .WithDescription("Updates notification information by ID")
                .Accepts<Notification>("application/json")
                .Produces<ApiResult<Notification>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // DELETE /api/notifications/{notificationId}
            group.MapDelete("/{notificationId}", DeleteNotification)
                .WithName("DeleteNotification")
                .WithSummary("Delete a notification")
                .WithDescription("Removes a notification record from the system")
                .Produces<ApiResponse>(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            return app;
        }

        /// <summary>
        /// Handles GET /api/notifications - Retrieve all notifications.
        /// </summary>
        private static async Task<IResult> GetAllNotifications(INotificationService service)
        {
            var notifications = await service.GetAllNotificationsAsync();
            return Results.Ok(ApiResult<List<Notification>>.SuccessResult(
                notifications,
                "Notifications retrieved successfully"));
        }

        /// <summary>
        /// Handles GET /api/notifications/{notificationId} - Retrieve a specific notification.
        /// </summary>
        private static async Task<IResult> GetNotificationById(int notificationId, INotificationService service)
        {
            if (notificationId <= 0)
                return Results.BadRequest(ApiResult<Notification>.FailureResult(
                    "Invalid notification ID. Must be greater than 0."));

            var notification = await service.GetNotificationByIdAsync(notificationId);

            if (notification == null)
                return Results.NotFound(ApiResult<Notification>.FailureResult(
                    $"Notification with ID {notificationId} not found."));

            return Results.Ok(ApiResult<Notification>.SuccessResult(
                notification,
                "Notification retrieved successfully"));
        }

        /// <summary>
        /// Handles POST /api/notifications - Create a new notification.
        /// </summary>
        private static async Task<IResult> CreateNotification(Notification notification, INotificationService service)
        {
            var createdNotification = await service.CreateNotificationAsync(notification);

            return Results.CreatedAtRoute(
                "GetNotificationById",
                new { notificationId = createdNotification.NotificationID },
                ApiResult<Notification>.SuccessResult(
                    createdNotification,
                    "Notification created successfully"));
        }

        /// <summary>
        /// Handles PUT /api/notifications/{notificationId} - Update an existing notification.
        /// </summary>
        private static async Task<IResult> UpdateNotification(int notificationId, Notification notificationUpdate, INotificationService service)
        {
            if (notificationId <= 0)
                return Results.BadRequest(ApiResult<Notification>.FailureResult(
                    "Invalid notification ID. Must be greater than 0."));

            var updatedNotification = await service.UpdateNotificationAsync(notificationId, notificationUpdate);

            return Results.Ok(ApiResult<Notification>.SuccessResult(
                updatedNotification,
                "Notification updated successfully"));
        }

        /// <summary>
        /// Handles DELETE /api/notifications/{notificationId} - Delete a notification.
        /// </summary>
        private static async Task<IResult> DeleteNotification(int notificationId, INotificationService service)
        {
            if (notificationId <= 0)
                return Results.BadRequest(ApiResult<Notification>.FailureResult(
                    "Invalid notification ID. Must be greater than 0."));

            await service.DeleteNotificationAsync(notificationId);

            return Results.NoContent();
        }
    }
}