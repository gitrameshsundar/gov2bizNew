using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LicenseManagement.DTO;
namespace LicenseManagement.Pages
{
    public class TenantsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public List<TenantDto> Tenants { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public TenantsModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            await LoadTenants();
        }

        public async Task<IActionResult> OnPostSaveTenantAsync(int tenantId, string tenantName)
        {
            if (string.IsNullOrWhiteSpace(tenantName))
            {
                ErrorMessage = "Tenant name is required.";
                await LoadTenants();
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("GatewayAPI"); client.BaseAddress = new Uri(_configuration["APISettings:BaseUrl"]);
                var tenant = new { name = tenantName };
                var content = new StringContent(JsonSerializer.Serialize(tenant), Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                
                if (tenantId > 0)
                {
                    response = await client.PutAsync($"api/tenants/{tenantId}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "Tenant updated successfully!";
                    }
                }
                else
                {
                    response = await client.PostAsync("api/tenants", content);
                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "Tenant created successfully!";
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Error saving tenant: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            await LoadTenants();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteTenantAsync(int tenantId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI"); client.BaseAddress = new Uri(_configuration["APISettings:BaseUrl"]);
                var response = await client.DeleteAsync($"api/tenants/{tenantId}");

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Tenant deleted successfully!";
                }
                else
                {
                    ErrorMessage = $"Error deleting tenant: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            await LoadTenants();
            return Page();
        }

        private async Task LoadTenants()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI"); client.BaseAddress = new Uri(_configuration["APISettings:BaseUrl"]);
                var response = await client.GetAsync("api/tenants");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResult<List<TenantDto>>>(jsonContent, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
                    Tenants = result?.Data ?? new();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = "Unauthorized: Please log in to access tenants.";
                }
                else
                {
                    ErrorMessage = "Unable to load tenants. Please try again later.";
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
    }



}