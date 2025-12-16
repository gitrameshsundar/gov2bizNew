using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LicenseManagement.DTO;
namespace LicenseManagement.Pages
{
    public class LicensesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public List<LicenseDto> Licenses { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public LicensesModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            await LoadLicenses();
        }

        public async Task<IActionResult> OnPostSaveLicenseAsync(int licenseId, string licenseName)
        {
            if (string.IsNullOrWhiteSpace(licenseName))
            {
                ErrorMessage = "License name is required.";
                await LoadLicenses();
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("GatewayAPI"); client.BaseAddress = new Uri(_configuration["APISettings:BaseUrl"]);
                var license = new { name = licenseName };
                var content = new StringContent(JsonSerializer.Serialize(license), Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                
                if (licenseId > 0)
                {
                    response = await client.PutAsync($"api/licenses/{licenseId}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "License updated successfully!";
                    }
                }
                else
                {
                    response = await client.PostAsync("api/licenses", content);
                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "License created successfully!";
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Error saving license: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            await LoadLicenses();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteLicenseAsync(int licenseId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI"); client.BaseAddress = new Uri(_configuration["APISettings:BaseUrl"]);
                var response = await client.DeleteAsync($"api/licenses/{licenseId}");

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "License deleted successfully!";
                }
                else
                {
                    ErrorMessage = $"Error deleting license: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            await LoadLicenses();
            return Page();
        }

        private async Task LoadLicenses()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI"); client.BaseAddress = new Uri(_configuration["APISettings:BaseUrl"]);
                var response = await client.GetAsync("api/licenses");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResult<List<LicenseDto>>>(jsonContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    Licenses = result?.Data ?? new();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = "Unauthorized: Please log in to access licenses.";
                }
                else
                {
                    ErrorMessage = "Unable to load licenses. Please try again later.";
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