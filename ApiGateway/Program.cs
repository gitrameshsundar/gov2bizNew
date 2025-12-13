using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Gateway",
        Version = "v1",
        Description = "Central API Gateway for microservices"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

if (string.IsNullOrEmpty(secretKey))
    throw new InvalidOperationException("JwtSettings:SecretKey is not configured");

var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = 401;
                return context.Response.WriteAsJsonAsync(new { message = "Unauthorized: Invalid or expired token" });
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                return context.Response.WriteAsJsonAsync(new { message = "Unauthorized: Token required" });
            }
        };
    });

builder.Services.AddAuthorization();

// Add Ocelot
builder.Services.AddOcelot();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Force HTTPS in all environments
app.UseHttpsRedirection();

// Configure Swagger BEFORE auth endpoints
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");

// Use routing BEFORE Ocelot so local endpoints get matched first
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map local endpoints (these need routing enabled)
app.UseEndpoints(endpoints =>
{
    endpoints.MapPost("/api/auth/login", Login)
        .WithName("Login")
        .WithSummary("Login and get JWT token")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

    endpoints.MapPost("/api/auth/refresh", RefreshToken)
        .WithName("RefreshToken")
        .WithSummary("Refresh JWT token")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

    endpoints.MapGet("/", RootHandler)
        .WithName("Root")
        .WithSummary("API Gateway root endpoint")
        .AllowAnonymous();

    endpoints.MapGet("/health", HealthHandler)
        .WithName("HealthCheck")
        .WithSummary("API Gateway health check")
        .AllowAnonymous();

    endpoints.MapGet("/gateway/info", GatewayInfoHandler)
        .WithName("GatewayInfo")
        .WithSummary("Get API Gateway information")
        .AllowAnonymous();
});

// Use Ocelot middleware LAST - it will catch all remaining routes
await app.UseOcelot();

app.Run();

// Handler functions (moved outside of UseEndpoints for clarity)
async Task<IResult> Login([FromBody] LoginRequest request, [FromServices] IConfiguration config)
{
    var jwtSettings = config.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];
    var issuer = jwtSettings["Issuer"];
    var audience = jwtSettings["Audience"];
    var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

    var keyBytes = Encoding.ASCII.GetBytes(secretKey);
    var tokenHandler = new JwtSecurityTokenHandler();

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Name, request.Username),
        new Claim("sub", request.Username),
        new Claim(ClaimTypes.Role, "Admin")
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
        Issuer = issuer,
        Audience = audience,
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var accessToken = tokenHandler.WriteToken(token);

    return Results.Ok(new AuthResponse
    {
        AccessToken = accessToken,
        ExpiresIn = expirationMinutes * 60,
        TokenType = "Bearer"
    });
}

async Task<IResult> RefreshToken([FromBody] RefreshTokenRequest request, [FromServices] IConfiguration config)
{
    try
    {
        var jwtSettings = config.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var keyBytes = Encoding.ASCII.GetBytes(secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(request.AccessToken, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        }, out SecurityToken validatedToken);

        var claims = principal.Claims.ToList();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var newToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(newToken);

        return Results.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresIn = expirationMinutes * 60,
            TokenType = "Bearer"
        });
    }
    catch
    {
        return Results.Unauthorized();
    }
}

async Task<IResult> RootHandler()
{
    return Results.Ok(new
    {
        message = "API Gateway",
        version = "1.0",
        swaggerUrl = "/swagger/index.html"
    });
}

async Task<IResult> HealthHandler()
{
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}

async Task<IResult> GatewayInfoHandler()
{
    return Results.Ok(new
    {
        name = "API Gateway",
        version = "1.0",
        services = new[]
        {
            new { name = "Customers Service", url = "https://localhost:7084" }
        }
    });
}

class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
}

class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}