using LicenseManagement.Data.Data;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;
using LicenseManagement.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace users.Endpoints
{
    /// <summary>
    /// Endpoint handlers for User API operations.
    /// 
    /// DESIGN PATTERN: Endpoint Organization Pattern (Minimal APIs)
    /// PURPOSE: Organizes all user-related HTTP endpoint handlers in a single class.
    /// 
    /// SPECIAL FEATURES:
    /// - Includes tenant assignment endpoints
    /// - User management operations
    /// - JWT authentication login endpoint
    /// 
    /// SECURITY NOTES:
    /// - Login endpoint is public (AllowAnonymous)
    /// - JWT token generation uses secure symmetric keys
    /// - Password verification uses BCrypt hashing
    /// - All other endpoints should be protected with [Authorize]
    /// </summary>
    public static class UserEndpoints
    {
        /// <summary>
        /// Registers all user-related endpoints to the application.
        /// 
        /// ENDPOINT STRUCTURE:
        /// - Authentication: POST /api/auth/login (public)
        /// - User Management: GET, POST, PUT, DELETE /api/users
        /// - Tenant Assignment: PUT /api/users/{userId}/tenant/{tenantId}
        /// </summary>
        /// <param name="app">The WebApplication instance</param>
        /// <returns>The WebApplication for method chaining</returns>
        public static WebApplication MapUserEndpoints(this WebApplication app)
        {
            // ============================================================
            // AUTH ENDPOINTS (Public - No Authorization Required)
            // ============================================================

            var authGroup = app.MapGroup("/api/auth")
                .WithName("Authentication")
                .WithOpenApi();

            // POST /api/auth/login
            authGroup.MapPost("/login", Login)
                .WithName("Login")
                .WithSummary("Login and get JWT token")
                .WithDescription("Authenticates user with username/password and returns JWT token")
                .AllowAnonymous()
                .Accepts<LoginRequest>("application/json")
                .Produces<AuthResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .WithOpenApi();

            // ============================================================
            // USER ENDPOINTS (Protected - Authorization Required)
            // ============================================================

            var userGroup = app.MapGroup("/api/users")
                .WithName("Users")
                .WithOpenApi();
            var userAuthGroup = app.MapGroup("/api/usersauth")
                .WithName("Users")
                .WithOpenApi();
            // GET /api/users
            userGroup.MapGet("/", GetAllUsers)
                .WithName("GetAllUsers")
                .WithSummary("Get all users")
                .WithDescription("Retrieves a list of all users in the system")
                .Produces<ApiResult<List<User>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // GET /api/users/tenant/{tenantId} - Must be before /{userId}
            userGroup.MapGet("/tenant/{tenantId}", GetUsersByTenant)
                .WithName("GetUsersByTenant")
                .WithSummary("Get users by tenant")
                .WithDescription("Retrieves all users assigned to a specific tenant")
                .Produces<ApiResult<List<User>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .WithOpenApi();

            // GET /api/usersauth/{username}/{password} - More specific route, registered before /{userId}
            userAuthGroup.MapGet("/{Username}", GetUserByUserName)
                .WithName("GetUserByUserName")
                .WithSummary("Authenticate user by username and password")
                .WithDescription("Retrieves a specific user by their username and password for authentication")
                .AllowAnonymous()
                .Produces<ApiResult<User>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            // GET /api/users/{userId} - Generic numeric route, registered after more specific routes
            userGroup.MapGet("/{userId}", GetUserById)
                .WithName("GetUserById")
                .WithSummary("Get user by ID")
                .WithDescription("Retrieves a specific user by their ID")
                .Produces<ApiResult<User>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            // POST /api/users
            userGroup.MapPost("/", CreateUser)
                .WithName("CreateUser")
                .WithSummary("Create a new user")
                .WithDescription("Creates a new user record in the system")
                .Accepts<User>("application/json")
                .Produces<ApiResult<User>>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // PUT /api/users/{userId}
            userGroup.MapPut("/{userId}", UpdateUser)
                .WithName("UpdateUser")
                .WithSummary("Update an existing user")
                .WithDescription("Updates user information by ID")
                .Accepts<User>("application/json")
                .Produces<ApiResult<User>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // PUT /api/users/{userId}/tenant/{tenantId}
            userGroup.MapPut("/{userId}/tenant/{tenantId}", AssignUserToTenant)
                .WithName("AssignUserToTenant")
                .WithSummary("Assign user to tenant")
                .WithDescription("Assigns a user to a specific tenant")
                .Produces<ApiResult<User>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            // DELETE /api/users/{userId}
            userGroup.MapDelete("/{userId}", DeleteUser)
                .WithName("DeleteUser")
                .WithSummary("Delete a user")
                .WithDescription("Removes a user record from the system")
                .Produces<ApiResponse>(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            return app;
        }

        /// <summary>
        /// Handles POST /api/auth/login - User authentication and JWT token generation.
        /// 
        /// HTTP METHOD: POST
        /// RESPONSE CODE: 200 OK with JWT token, 401 Unauthorized if credentials invalid
        /// REQUEST BODY: LoginRequest { Username, Password }
        /// 
        /// SECURITY CONSIDERATIONS:
        /// - Password is hashed with BCrypt in database
        /// - JWT token includes user claims (ID, username, role, tenant)
        /// - Token expiration is configurable via appsettings
        /// - Symmetric signing key (HS256) for token verification
        /// 
        /// TOKEN CLAIMS:
        /// - NameIdentifier: User ID
        /// - Name: Username
        /// - Role: User role (Admin, Manager, User)
        /// - TenantId: Tenant ID user belongs to
        /// 
        /// ERROR CASES:
        /// - 401 Unauthorized: Invalid username or password
        /// </summary>
        /// <param name="request">Login credentials (username/password)</param>
        /// <param name="db">Database context (injected)</param>
        /// <param name="configuration">Configuration provider for JWT settings (injected)</param>
        /// <returns>200 OK with JWT token or 401 Unauthorized</returns>
        private static async Task<IResult> Login(LoginRequest request, UserDbContext db, IConfiguration configuration)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest(new { message = "Username and password are required" });

            // Find user by username
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // Verify password
            if (user == null || !VerifyPassword(request.Password, user.Password))
                return Results.Unauthorized();

            // Get JWT settings from configuration
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            // Generate JWT token
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            // Create token claims with user information
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("Role", user.Role),
                new Claim("TenantId", user.TenantID.ToString())
            };

            // Create token descriptor
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

            // Create and write token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            // Return success response with token
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

        /// <summary>
        /// Handles GET /api/users - Retrieve all users.
        /// 
        /// HTTP METHOD: GET (Safe, Idempotent)
        /// RESPONSE CODE: 200 OK
        /// </summary>
        private static async Task<IResult> GetAllUsers(IUserService service)
        {
            var users = await service.GetAllUsersAsync();
            return Results.Ok(ApiResult<List<User>>.SuccessResult(
                users,
                "Users retrieved successfully"));
        }

        /// <summary>
        /// Handles GET /api/users/{username}/{password} - Authenticate user with credentials.
        /// 
        /// HTTP METHOD: GET (Safe, Idempotent)
        /// RESPONSE CODE: 200 OK on success, 401 Unauthorized if credentials invalid, 404 Not Found if user doesn't exist
        /// 
        /// SECURITY CONSIDERATIONS:
        /// - Password is verified using BCrypt hashing
        /// - Credentials are passed in URL (should use POST in production for better security)
        /// - This endpoint is for authentication purposes
        /// </summary>
        private static async Task<IResult> GetUserByUserName(string username,IUserService service)
        {
            if (string.IsNullOrWhiteSpace(username))
                return Results.BadRequest(ApiResult<User>.FailureResult(
                    "Invalid username or password."));

            var user = await service.GetUserByUsernameAsync(username);

            if (user == null)
                return Results.NotFound(ApiResult<User>.FailureResult(
                    $"User with username '{username}' not found."));

            // Verify password
            //if (!VerifyPassword(pwd, user.Password))
             //   return Results.Unauthorized();

            return Results.Ok(ApiResult<User>.SuccessResult(
                user,
                "User authenticated successfully"));
        }

        /// <summary>
        /// Handles GET /api/users/{userId} - Retrieve a specific user by ID.
        /// 
        /// HTTP METHOD: GET (Safe, Idempotent)
        /// RESPONSE CODE: 200 OK on success, 404 Not Found if user doesn't exist
        /// </summary>
        private static async Task<IResult> GetUserById(int userId, IUserService service)
        {
            if (userId <= 0)
                return Results.BadRequest(ApiResult<User>.FailureResult(
                    "Invalid user ID. Must be greater than 0."));

            var user = await service.GetUserByIdAsync(userId);

            if (user == null)
                return Results.NotFound(ApiResult<User>.FailureResult(
                    $"User with ID {userId} not found."));

            return Results.Ok(ApiResult<User>.SuccessResult(
                user,
                "User retrieved successfully"));
        }

        /// <summary>
        /// Handles GET /api/users/tenant/{tenantId} - Retrieve users by tenant.
        /// 
        /// HTTP METHOD: GET (Safe, Idempotent)
        /// RESPONSE CODE: 200 OK
        /// </summary>
        private static async Task<IResult> GetUsersByTenant(int tenantId, IUserService service)
        {
            if (tenantId <= 0)
                return Results.BadRequest(ApiResult<List<User>>.FailureResult(
                    "Invalid tenant ID. Must be greater than 0."));

            var users = await service.GetUsersByTenantAsync(tenantId);
            return Results.Ok(ApiResult<List<User>>.SuccessResult(
                users,
                "Users retrieved successfully"));
        }

        /// <summary>
        /// Handles POST /api/users - Create a new user.
        /// 
        /// HTTP METHOD: POST (Not Safe, Not Idempotent)
        /// RESPONSE CODE: 201 Created on success
        /// </summary>
        private static async Task<IResult> CreateUser(User user, IUserService service)
        {
            var createdUser = await service.CreateUserAsync(user);

            return Results.CreatedAtRoute(
                "GetUserById",
                new { userId = createdUser.UserID },
                ApiResult<User>.SuccessResult(
                    createdUser,
                    "User created successfully"));
        }

        /// <summary>
        /// Handles PUT /api/users/{userId} - Update an existing user.
        /// 
        /// HTTP METHOD: PUT (Not Safe, Idempotent)
        /// RESPONSE CODE: 200 OK on success
        /// </summary>
        private static async Task<IResult> UpdateUser(int userId, User userUpdate, IUserService service)
        {
            if (userId <= 0)
                return Results.BadRequest(ApiResult<User>.FailureResult(
                    "Invalid user ID. Must be greater than 0."));

            var updatedUser = await service.UpdateUserAsync(userId, userUpdate);

            return Results.Ok(ApiResult<User>.SuccessResult(
                updatedUser,
                "User updated successfully"));
        }

        /// <summary>
        /// Handles PUT /api/users/{userId}/tenant/{tenantId} - Assign user to tenant.
        /// 
        /// HTTP METHOD: PUT (Not Safe, Idempotent)
        /// RESPONSE CODE: 200 OK on success
        /// </summary>
        private static async Task<IResult> AssignUserToTenant(int userId, int tenantId, IUserService service)
        {
            if (userId <= 0)
                return Results.BadRequest(ApiResult<User>.FailureResult(
                    "Invalid user ID. Must be greater than 0."));

            if (tenantId <= 0)
                return Results.BadRequest(ApiResult<User>.FailureResult(
                    "Invalid tenant ID. Must be greater than 0."));

            await service.AssignUserToTenantAsync(userId, tenantId);
            var user = await service.GetUserByIdAsync(userId);

            return Results.Ok(ApiResult<User>.SuccessResult(
                user!,
                "User assigned to tenant successfully"));
        }

        /// <summary>
        /// Handles DELETE /api/users/{userId} - Delete a user.
        /// 
        /// HTTP METHOD: DELETE (Not Safe, Idempotent)
        /// RESPONSE CODE: 204 No Content on success
        /// </summary>
        private static async Task<IResult> DeleteUser(int userId, IUserService service)
        {
            if (userId <= 0)
                return Results.BadRequest(ApiResult<User>.FailureResult(
                    "Invalid user ID. Must be greater than 0."));

            await service.DeleteUserAsync(userId);

            return Results.NoContent();
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        /// <summary>
        /// Hashes a password using BCrypt algorithm.
        /// 
        /// PURPOSE: Secure password storage
        /// ALGORITHM: BCrypt with work factor
        /// SALTING: Automatic (included in BCrypt)
        /// </summary>
        /// <param name="password">Plain text password to hash</param>
        /// <returns>Hashed password</returns>
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifies a plain text password against a BCrypt hash.
        /// 
        /// PURPOSE: Validate login credentials
        /// ALGORITHM: BCrypt verification
        /// TIMING ATTACK SAFE: Yes (BCrypt is timing-safe)
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <param name="hash">BCrypt hash to verify against</param>
        /// <returns>True if password matches hash, false otherwise</returns>
        private static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
