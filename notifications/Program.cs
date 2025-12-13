using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

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

var notificationGroup = app.MapGroup("/api/notifications")
    .WithName("Notifications");
    //.RequireAuthorization();

notificationGroup.MapGet("/", GetAllNotifications)
    .WithName("GetAllNotifications")
    .WithSummary("Get all notifications")
    .Produces<List<Notification>>(StatusCodes.Status200OK);

notificationGroup.MapGet("/{notificationId}", GetNotificationById)
    .WithName("GetNotificationById")
    .WithSummary("Get notification by ID")
    .Produces<Notification>(StatusCodes.Status200OK);

notificationGroup.MapPost("/", CreateNotification)
    .WithName("CreateNotification")
    .WithSummary("Create a new notification")
    .Produces<Notification>(StatusCodes.Status201Created);

notificationGroup.MapPut("/{notificationId}", UpdateNotification)
    .WithName("UpdateNotification")
    .WithSummary("Update a notification")
    .Produces<Notification>(StatusCodes.Status200OK);

notificationGroup.MapDelete("/{notificationId}", DeleteNotification)
    .WithName("DeleteNotification")
    .WithSummary("Delete a notification")
    .Produces(StatusCodes.Status204NoContent);

app.Run();

async Task<IResult> GetAllNotifications(NotificationDbContext db)
{
    var notifications = await db.Notifications.ToListAsync();
    return Results.Ok(notifications);
}

async Task<IResult> GetNotificationById(int notificationId, NotificationDbContext db)
{
    var notification = await db.Notifications.FirstOrDefaultAsync(n => n.NotificationID == notificationId);
    if (notification == null)
        return Results.NotFound(new { message = "Notification not found" });
    return Results.Ok(notification);
}

async Task<IResult> CreateNotification(Notification notification, NotificationDbContext db)
{
    db.Notifications.Add(notification);
    await db.SaveChangesAsync();
    return Results.CreatedAtRoute("GetNotificationById", new { notificationId = notification.NotificationID }, notification);
}

async Task<IResult> UpdateNotification(int notificationId, Notification notificationUpdate, NotificationDbContext db)
{
    var notification = await db.Notifications.FirstOrDefaultAsync(n => n.NotificationID == notificationId);
    if (notification == null)
        return Results.NotFound(new { message = "Notification not found" });

    notification.Title = notificationUpdate.Title;
    notification.Message = notificationUpdate.Message;
    notification.Status = notificationUpdate.Status;
    await db.SaveChangesAsync();
    return Results.Ok(notification);
}

async Task<IResult> DeleteNotification(int notificationId, NotificationDbContext db)
{
    var notification = await db.Notifications.FirstOrDefaultAsync(n => n.NotificationID == notificationId);
    if (notification == null)
        return Results.NotFound(new { message = "Notification not found" });

    db.Notifications.Remove(notification);
    await db.SaveChangesAsync();
    return Results.NoContent();
}

class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }
    public DbSet<Notification> Notifications { get; set; } = null!;
}

class Notification
{
    public int NotificationID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "Unread";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
