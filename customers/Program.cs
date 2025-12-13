using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
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

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

if (string.IsNullOrEmpty(secretKey))
    throw new InvalidOperationException("JwtSettings:SecretKey is not configured");

var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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
        //options.Events = new JwtBearerEvents
        //{
        //    OnAuthenticationFailed = context =>
        //    {
        //        context.Response.StatusCode = 401;
        //        return context.Response.WriteAsJsonAsync(new { message = "Unauthorized" });
        //    }
        //};
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Auth Endpoints (No Authorization Required)
app.MapPost("/api/auth/login", Login)
    .WithName("Login")
    .WithSummary("Login and get JWT token")
    .AllowAnonymous()
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized);

app.MapPost("/api/auth/refresh", RefreshToken)
    .WithName("RefreshToken")
    .WithSummary("Refresh JWT token")
    .AllowAnonymous()
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized);

// CRUD Endpoints for Customers (Authorization Required)
var customerGroup = app.MapGroup("/api/customers")
    .WithName("Customers")
    .RequireAuthorization();

// GET all customers
customerGroup.MapGet("/", GetAllCustomers)
    .WithName("GetAllCustomers")
    .WithSummary("Get all customers")
    .Produces<List<Customer>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized);

// GET customer by ID
customerGroup.MapGet("/{customerId}", GetCustomerById)
    .WithName("GetCustomerById")
    .WithSummary("Get customer by ID")
    .Produces<Customer>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized);

// POST create customer
customerGroup.MapPost("/", CreateCustomer)
    .WithName("CreateCustomer")
    .WithSummary("Create a new customer")
    .Accepts<Customer>("application/json")
    .Produces<Customer>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized);

// PUT update customer
customerGroup.MapPut("/{customerId}", UpdateCustomer)
    .WithName("UpdateCustomer")
    .WithSummary("Update an existing customer")
    .Accepts<Customer>("application/json")
    .Produces<Customer>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized);

// DELETE customer
customerGroup.MapDelete("/{customerId}", DeleteCustomer)
    .WithName("DeleteCustomer")
    .WithSummary("Delete a customer")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized);

app.Run();

// Auth Handlers
async Task<IResult> Login(LoginRequest request, IConfiguration config)
{
    // Demo credentials - replace with actual user validation
    if (request.Username != "string" || request.Password != "string")
        return Results.Unauthorized();

    var jwtSettings = config.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];
    var issuer = jwtSettings["Issuer"];
    var audience = jwtSettings["Audience"];
    var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

    var key = Encoding.ASCII.GetBytes(secretKey);
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
            new SymmetricSecurityKey(key),
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

async Task<IResult> RefreshToken(RefreshTokenRequest request, IConfiguration config)
{
    try
    {
        var jwtSettings = config.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        // Validate old token
        var principal = tokenHandler.ValidateToken(request.AccessToken, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
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
                new SymmetricSecurityKey(key),
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

// Endpoint handlers
async Task<IResult> GetAllCustomers(CustomerDbContext db)
{
    var customers = await db.Customers.ToListAsync();
    return Results.Ok(customers);
}

async Task<IResult> GetCustomerById(int customerId, CustomerDbContext db)
{
    var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerID == customerId);
    if (customer == null)
        return Results.NotFound(new { message = "Customer not found" });

    return Results.Ok(customer);
}

async Task<IResult> CreateCustomer(Customer customer, CustomerDbContext db)
{
    if (string.IsNullOrWhiteSpace(customer.Name))
        return Results.BadRequest(new { message = "Customer name is required" });

    db.Customers.Add(customer);
    await db.SaveChangesAsync();
    return Results.CreatedAtRoute("GetCustomerById", new { customerId = customer.CustomerID }, customer);
}

async Task<IResult> UpdateCustomer(int customerId, Customer customerUpdate, CustomerDbContext db)
{
    if (string.IsNullOrWhiteSpace(customerUpdate.Name))
        return Results.BadRequest(new { message = "Customer name is required" });

    var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerID == customerId);
    if (customer == null)
        return Results.NotFound(new { message = "Customer not found" });

    customer.Name = customerUpdate.Name;
    await db.SaveChangesAsync();
    return Results.Ok(customer);
}

async Task<IResult> DeleteCustomer(int customerId, CustomerDbContext db)
{
    var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerID == customerId);
    if (customer == null)
        return Results.NotFound(new { message = "Customer not found" });

    db.Customers.Remove(customer);
    await db.SaveChangesAsync();
    return Results.NoContent();
}

// DbContext
class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerID);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
        });
    }
}

// Models
class Customer
{
    public int CustomerID { get; set; }
    public string Name { get; set; } = string.Empty;
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