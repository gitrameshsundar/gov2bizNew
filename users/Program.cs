using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Users API",
        Version = "v1",
        Description = "API for managing users and tenant assignments"
    });
});

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Repository & Service
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Add logging
builder.Services.AddLogging();

// Register Repository & Service
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Add logging
builder.Services.AddLogging();

// Register Repository & Service
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Add logging
builder.Services.AddLogging();

// Register Repository & Service
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Add logging
builder.Services.AddLogging();

// Register Repository & Service
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Add logging
builder.Services.AddLogging();

// Register Repository & Service
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Add logging
builder.Services.AddLogging();

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

// Auth Endpoints
app.MapPost("/api/auth/login", Login)
    .WithName("Login")
    .WithSummary("Login and get JWT token")
    .AllowAnonymous()
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized);

// User Endpoints
var userGroup = app.MapGroup("/api/users")
    .WithName("Users");

userGroup.MapGet("/", GetAllUsers)
    .WithName("GetAllUsers")
    .WithSummary("Get all users")
    .Produces<List<User>>(StatusCodes.Status200OK);

userGroup.MapGet("/{userId}", GetUserById)
    .WithName("GetUserById")
    .WithSummary("Get user by ID")
    .Produces<User>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

userGroup.MapPost("/", CreateUser)
    .WithName("CreateUser")
    .WithSummary("Create a new user")
    .Accepts<User>("application/json")
    .Produces<User>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

userGroup.MapPut("/{userId}", UpdateUser)
    .WithName("UpdateUser")
    .WithSummary("Update an existing user")
    .Accepts<User>("application/json")
    .Produces<User>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

userGroup.MapDelete("/{userId}", DeleteUser)
    .WithName("DeleteUser")
    .WithSummary("Delete a user")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

userGroup.MapGet("/tenant/{tenantId}", GetUsersByTenant)
    .WithName("GetUsersByTenant")
    .WithSummary("Get all users assigned to a tenant")
    .Produces<List<User>>(StatusCodes.Status200OK);

userGroup.MapPut("/{userId}/tenant/{tenantId}", AssignUserToTenant)
    .WithName("AssignUserToTenant")
    .WithSummary("Assign a user to a tenant")
    .Produces<User>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.Run();

// Auth Handler
async Task<IResult> Login(LoginRequest request, UserDbContext db)
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

    if (user == null || !VerifyPassword(request.Password, user.Password))
        return Results.Unauthorized();

    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];
    var issuer = jwtSettings["Issuer"];
    var audience = jwtSettings["Audience"];
    var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

    var key = Encoding.ASCII.GetBytes(secretKey);
    var tokenHandler = new JwtSecurityTokenHandler();

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim("Role", user.Role),
        new Claim("TenantId", user.TenantID.ToString())
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
        Issuer = issuer,
        Audience = audience,
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var accessToken = tokenHandler.WriteToken(token);

    return Results.Ok(new AuthResponse
    {
        AccessToken = accessToken,
        ExpiresIn = expirationMinutes * 60,
        TokenType = "Bearer",
        UserId = user.UserID,
        Username = user.Username,
        Role = user.Role
    });
}

// User Endpoint Handlers
async Task<IResult> GetAllUsers(UserDbContext db)
{
    var users = await db.Users
        .Select(u => new
        {
            u.UserID,
            u.Username,
            u.Email,
            u.Role,
            u.TenantID,
            u.CreatedDate
        })
        .ToListAsync();
    return Results.Ok(users);
}

async Task<IResult> GetUserById(int userId, UserDbContext db)
{
    var user = await db.Users
        .Where(u => u.UserID == userId)
        .Select(u => new
        {
            u.UserID,
            u.Username,
            u.Email,
            u.Role,
            u.TenantID,
            u.CreatedDate
        })
        .FirstOrDefaultAsync();

    if (user == null)
        return Results.NotFound(new { message = "User not found" });

    return Results.Ok(user);
}

async Task<IResult> CreateUser(User user, UserDbContext db)
{
    if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
        return Results.BadRequest(new { message = "Username and email are required" });

    var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
    if (existingUser != null)
        return Results.BadRequest(new { message = "Username already exists" });

    user.Password = HashPassword(user.Password ?? "User@123");
    user.CreatedDate = DateTime.UtcNow;

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.CreatedAtRoute("GetUserById", new { userId = user.UserID }, new
    {
        user.UserID,
        user.Username,
        user.Email,
        user.Role,
        user.TenantID,
        user.CreatedDate
    });
}

async Task<IResult> UpdateUser(int userId, User userUpdate, UserDbContext db)
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.UserID == userId);
    if (user == null)
        return Results.NotFound(new { message = "User not found" });

    if (!string.IsNullOrWhiteSpace(userUpdate.Email))
        user.Email = userUpdate.Email;

    if (!string.IsNullOrWhiteSpace(userUpdate.Role))
        user.Role = userUpdate.Role;

    if (userUpdate.TenantID > 0)
        user.TenantID = userUpdate.TenantID;

    user.UpdatedDate = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        user.UserID,
        user.Username,
        user.Email,
        user.Role,
        user.TenantID,
        user.CreatedDate,
        user.UpdatedDate
    });
}

async Task<IResult> DeleteUser(int userId, UserDbContext db)
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.UserID == userId);
    if (user == null)
        return Results.NotFound(new { message = "User not found" });

    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.NoContent();
}

async Task<IResult> GetUsersByTenant(int tenantId, UserDbContext db)
{
    var users = await db.Users
        .Where(u => u.TenantID == tenantId)
        .Select(u => new
        {
            u.UserID,
            u.Username,
            u.Email,
            u.Role,
            u.TenantID,
            u.CreatedDate
        })
        .ToListAsync();

    return Results.Ok(users);
}

async Task<IResult> AssignUserToTenant(int userId, int tenantId, UserDbContext db)
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.UserID == userId);
    if (user == null)
        return Results.NotFound(new { message = "User not found" });

    user.TenantID = tenantId;
    user.UpdatedDate = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        user.UserID,
        user.Username,
        user.Email,
        user.Role,
        user.TenantID,
        user.CreatedDate,
        user.UpdatedDate
    });
}

// Helper functions
static string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password);
}

static bool VerifyPassword(string password, string hash)
{
    return BCrypt.Net.BCrypt.Verify(password, hash);
}



