using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

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
        public string Username { get; set; } = string.Empty;

        [BindProperty]
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
                var user = await client.GetAsync($"api/usersauth/{Username}");

                // Validate user exists and password is correct
                if (user == null)// || !VerifyPassword(Password, user.Password))
                {
                    ErrorMessage = "Invalid username or password.";
                    return Page();
                }

                // Create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Name, Username),
                    new Claim("Role",""),
                    new Claim("TenantId", "0") // Admin has access to all tenants
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
                HttpContext.Session.SetString("UserId", "1");
                HttpContext.Session.SetString("Username", Username);
                HttpContext.Session.SetString("Role", "Admin");

                return RedirectToPage("/Index");
            }

        }
    }
}