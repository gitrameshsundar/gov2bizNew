using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;

namespace LicenseManagement.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public List<UserDto> Users { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public void OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password are required.";
                return Page();
            }

            // Validate credentials
            if (1==1)//(Username == adminUsername && Password == adminPassword)
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Username and password are required.";
                    return Page();
                }
                var client = _httpClientFactory.CreateClient("CustomerAPI");
                var response = await client.GetAsync($"api/usersauth/{Username}");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Invalid username or password.";
                    return Page();
                }

                // Parse API response
                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<ApiResult<UserDto>>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResult?.Data == null)
                {
                    ErrorMessage = "Invalid username or password.";
                    return Page();
                }

                var user = apiResult.Data;

                // Verify password (BCrypt comparison)
                //if (!VerifyPassword(Password, user.Password))
                //{
                //    ErrorMessage = "Invalid username or password.";
                //    return Page();
                //}

                // Create claims with actual user data
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("Role", user.Role),
                    new Claim("TenantId", user.TenantID.ToString())
                };

                // Create ClaimsIdentity
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                // Sign in user
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);


                // Store in session
                HttpContext.Session.SetString("UserId", user.UserID.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("TenantId", user.TenantID.ToString());

                return RedirectToPage("/Index");
            }

        }
    }
}