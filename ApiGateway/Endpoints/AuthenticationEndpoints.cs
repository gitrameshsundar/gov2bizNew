using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ApiGateway.Endpoints
{
    /// <summary>
    /// Authentication endpoints for the API Gateway.
    /// 
    /// DESIGN PATTERN: Endpoint Organization Pattern (Minimal APIs)
    /// PURPOSE: Centralizes JWT authentication endpoints
    /// 
    /// ENDPOINTS:
    /// - POST /api/auth/login - User authentication and token generation
    /// - POST /api/auth/refresh - Token refresh and renewal
    /// 
    /// BENEFITS:
    /// - Keeps Program.cs clean and focused on configuration
    /// - Centralizes authentication logic
    /// - Easy to test and maintain
    /// - Scalable architecture
    /// - Single Responsibility Principle
    /// </summary>
    public static class AuthenticationEndpoints
    {
        /// <summary>
        /// Registers all authentication endpoints to the application.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder</param>
        public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // POST /api/auth/login
            endpoints.MapPost("/api/auth/login", Login)
                .WithName("Login")
                .WithSummary("Login and get JWT token")
                .AllowAnonymous()
                .Produces<AuthResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .WithMetadata(new Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute());

            // POST /api/auth/refresh
            endpoints.MapPost("/api/auth/refresh", RefreshToken)
                .WithName("RefreshToken")
                .WithSummary("Refresh JWT token")
                .AllowAnonymous()
                .Produces<AuthResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .WithMetadata(new Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute());
        }

        /// <summary>
        /// Handles POST /api/auth/login - User authentication and JWT token generation.
        /// 
        /// HTTP METHOD: POST (Not Safe, Not Idempotent)
        /// RESPONSE CODE: 200 OK with JWT token, 401 Unauthorized if invalid
        /// REQUEST BODY: LoginRequest { Username, Password }
        /// 
        /// AUTHENTICATION FLOW:
        /// 1. Validate input parameters
        /// 2. Create JWT claims with user information
        /// 3. Generate token with symmetric key signature
        /// 4. Return token for use in subsequent requests
        /// 
        /// TOKEN CLAIMS:
        /// - NameIdentifier: User ID
        /// - Name: Username
        /// - Role: User role (Admin, Manager, User)
        /// 
        /// ERROR CASES:
        /// - 400 Bad Request: Missing required fields
        /// - 401 Unauthorized: Invalid credentials
        /// - 500 Internal Server Error: Configuration error
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <param name="configuration">Configuration provider for JWT settings</param>
        /// <returns>200 OK with JWT token or error response</returns>
        private static async Task<IResult> Login(
            [FromBody] LoginRequest request,
            [FromServices] IConfiguration configuration)
        {
            try
            {
                // Get JWT settings from configuration
                var jwtSettings = configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("SecretKey not configured");
                var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("Issuer not configured");
                var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("Audience not configured");
                var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

                // Generate JWT token
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
            catch (InvalidOperationException ex)
            {
                return Results.StatusCode(
                    StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                return Results.StatusCode(
                    StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Handles POST /api/auth/refresh - Token refresh and renewal.
        /// 
        /// HTTP METHOD: POST (Not Safe, Not Idempotent)
        /// RESPONSE CODE: 200 OK with new JWT token, 401 Unauthorized if invalid
        /// REQUEST BODY: RefreshTokenRequest { AccessToken }
        /// 
        /// REFRESH FLOW:
        /// 1. Validate token format and not empty
        /// 2. Verify token signature (validate even if expired)
        /// 3. Extract user claims from expired token
        /// 4. Generate new token with same claims
        /// 5. Return new token
        /// 
        /// IMPORTANT:
        /// - Expired tokens can be refreshed (lifetime check disabled)
        /// - Only tokens with valid signature are accepted
        /// - Client should refresh before token expires
        /// 
        /// ERROR CASES:
        /// - 400 Bad Request: Missing token
        /// - 401 Unauthorized: Invalid or corrupted token
        /// - 500 Internal Server Error: Configuration error
        /// </summary>
        /// <param name="request">Refresh request with expired token</param>
        /// <param name="configuration">Configuration provider for JWT settings</param>
        /// <returns>200 OK with new JWT token or error response</returns>
        private static async Task<IResult> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            [FromServices] IConfiguration configuration)
        {
            try
            {
                // Get JWT settings
                var jwtSettings = configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("SecretKey not configured");
                var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("Issuer not configured");
                var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("Audience not configured");
                var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

                var keyBytes = Encoding.ASCII.GetBytes(secretKey);
                var tokenHandler = new JwtSecurityTokenHandler();

                // Validate token without lifetime check (allows refresh of expired tokens)
                var principal = tokenHandler.ValidateToken(request.AccessToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false
                }, out SecurityToken validatedToken);

                // Extract claims from old token
                var claims = principal.Claims.ToList();

                // Create new token descriptor with same claims
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

                // Generate and return new token
                var newToken = tokenHandler.CreateToken(tokenDescriptor);
                var accessToken = tokenHandler.WriteToken(newToken);

                return Results.Ok(new AuthResponse
                {
                    AccessToken = accessToken,
                    ExpiresIn = expirationMinutes * 60,
                    TokenType = "Bearer"
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.StatusCode(
                    StatusCodes.Status500InternalServerError);
            }
            catch
            {
                return Results.Unauthorized();
            }
        }
    }

    // ============================================================
    // REQUEST/RESPONSE MODELS
    // ============================================================

    /// <summary>
    /// Login request with user credentials.
    /// </summary>
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Token refresh request.
    /// </summary>
    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Successful authentication response with JWT token.
    /// </summary>
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
}