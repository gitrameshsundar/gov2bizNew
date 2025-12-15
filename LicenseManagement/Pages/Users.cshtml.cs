using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LicenseManagement.Pages
{
    [Authorize]
    public class UsersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public List<UserDto> Users { get; set; } = new();
        public List<TenantDto> Tenants { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public UsersModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is admin
            if (!User.HasClaim("Role", "Admin"))
            {
                return RedirectToPage("/AccessDenied");
            }

            await LoadUsers();
            await LoadTenants();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveUserAsync(int userId, string username, string email, string role, int tenantId)
        {
            // Check if user is admin
            if (!User.HasClaim("Role", "Admin"))
            {
                return RedirectToPage("/AccessDenied");
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
            {
                ErrorMessage = "Username and email are required.";
                await LoadUsers();
                await LoadTenants();
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI");
                var user = new { username, email, role, tenantId };
                var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                
                if (userId > 0)
                {
                    response = await client.PutAsync($"api/users/{userId}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "User updated successfully!";
                    }
                }
                else
                {
                    response = await client.PostAsync("api/users", content);
                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "User created successfully!";
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Error saving user: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            await LoadUsers();
            await LoadTenants();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(int userId)
        {
            // Check if user is admin
            if (!User.HasClaim("Role", "Admin"))
            {
                return RedirectToPage("/AccessDenied");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI");
                var response = await client.DeleteAsync($"api/users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "User deleted successfully!";
                }
                else
                {
                    ErrorMessage = $"Error deleting user: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            await LoadUsers();
            await LoadTenants();
            return Page();
        }

        private async Task LoadUsers()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI");
                var response = await client.GetAsync("api/users");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResult<List<UserDto>>>(jsonContent, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    Users = result?.Data ?? new();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = "Unauthorized to access users.";
                }
                else
                {
                    ErrorMessage = "Unable to load users. Please try again later.";
                }
            }
            catch (HttpRequestException)
            {
                ErrorMessage = "Could not connect to the API. Please ensure the API Gateway is running.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
        }

        private async Task LoadTenants()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI");
                var response = await client.GetAsync("api/tenants");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResult<List<TenantDto>>>(jsonContent, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    Tenants = result?.Data ?? new();
                }
            }
            catch
            {
                // Tenants optional, don't show error
            }
        }
    }

    public class UserDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public int TenantID { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

}