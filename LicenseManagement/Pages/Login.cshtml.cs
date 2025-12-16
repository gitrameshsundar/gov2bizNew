using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using LicenseManagement.DTO;
using LicenseManagement.Service;
namespace LicenseManagement.Pages
{
    public class LoginModel : PageModel
    {
        // Bind the View Model to handle form input
        [BindProperty]
        public LoginView LoginInput { get; set; }

        // Inject the Interface, not the Concrete Class
        private readonly IClientAPIService _apiService;

        // Constructor Injection
        public LoginModel(IClientAPIService apiService)
        {
            _apiService = apiService;
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
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (string.IsNullOrWhiteSpace(LoginInput.Username) || string.IsNullOrWhiteSpace(LoginInput.Password))
            {
                LoginInput.ErrorMessage = "Username and password are required.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(LoginInput.Username) || string.IsNullOrWhiteSpace(LoginInput.Password))
            {
                LoginInput.ErrorMessage = "Username and password are required.";
                return Page();
            }
                

            // Call the separate service class for the API logic
            var user = await _apiService.LoginAsync(LoginInput);

            if (user==null)
            {
                LoginInput.ErrorMessage = "Invalid username or password.";
                return Page();
            }

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