using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Data;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.Services;
using LicenseManagement.Data.Middleware;
using LicenseManagement.Data.Results;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notifications API",
        Version = "v1",
        Description = "API for managing notifications"
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
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

var notificationGroup = app.MapGroup("/api/notifications")
    .WithName("Notifications");

notificationGroup.MapGet("/", GetAllNotifications)
    .WithName("GetAllNotifications")
    .WithSummary("Get all notifications")
    .Produces<ApiResult<List<Notification>>>(StatusCodes.Status200OK);

notificationGroup.MapGet("/{notificationId}", GetNotificationById)
    .WithName("GetNotificationById")
    .WithSummary("Get notification by ID")
    .Produces<ApiResult<Notification>>(StatusCodes.Status200OK);

notificationGroup.MapPost("/", CreateNotification)
    .WithName("CreateNotification")
    .WithSummary("Create a new notification")
    .Produces<ApiResult<Notification>>(StatusCodes.Status201Created);

notificationGroup.MapPut("/{notificationId}", UpdateNotification)
    .WithName("UpdateNotification")
    .WithSummary("Update a notification")
    .Produces<ApiResult<Notification>>(StatusCodes.Status200OK);

notificationGroup.MapDelete("/{notificationId}", DeleteNotification)
    .WithName("DeleteNotification")
    .WithSummary("Delete a notification")
    .Produces<ApiResponse>(StatusCodes.Status204NoContent);

app.Run();

async Task<IResult> GetAllNotifications(INotificationService service)
{
    var notifications = await service.GetAllNotificationsAsync();
    return Results.Ok(ApiResult<List<Notification>>.SuccessResult(notifications, "Notifications retrieved successfully"));
}

async Task<IResult> GetNotificationById(int notificationId, INotificationService service)
{
    var notification = await service.GetNotificationByIdAsync(notificationId);
    if (notification == null)
        return Results.NotFound(ApiResult<Notification>.FailureResult("Notification not found"));
    return Results.Ok(ApiResult<Notification>.SuccessResult(notification, "Notification retrieved successfully"));
}

async Task<IResult> CreateNotification(Notification notification, INotificationService service)
{
    var createdNotification = await service.CreateNotificationAsync(notification);
    return Results.CreatedAtRoute("GetNotificationById", new { notificationId = createdNotification.NotificationID },
        ApiResult<Notification>.SuccessResult(createdNotification, "Notification created successfully"));
}

async Task<IResult> UpdateNotification(int notificationId, Notification notificationUpdate, INotificationService service)
{
    var notification = await service.UpdateNotificationAsync(notificationId, notificationUpdate);
    return Results.Ok(ApiResult<Notification>.SuccessResult(notification, "Notification updated successfully"));
}

async Task<IResult> DeleteNotification(int notificationId, INotificationService service)
{
    await service.DeleteNotificationAsync(notificationId);
    return Results.NoContent();
}

