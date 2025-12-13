using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LicenseManagement.Pages
{
    public class CustomersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public List<CustomerDto> Customers { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public CustomersModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            await LoadCustomers();
        }

        public async Task<IActionResult> OnPostSaveCustomerAsync(int customerId, string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                ErrorMessage = "Customer name is required.";
                await LoadCustomers();
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI");
                //var token = HttpContext.Session.GetString("AuthToken");
                
                //if (!string.IsNullOrEmpty(token))
                //{
                //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                //}

                var customer = new { name = customerName };
                var content = new StringContent(JsonSerializer.Serialize(customer), Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                
                if (customerId > 0)
                {
                    // Update existing customer
                    response = await client.PutAsync($"api/customers/{customerId}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "Customer updated successfully!";
                    }
                }
                else
                {
                    // Create new customer
                    response = await client.PostAsync("api/customers", content);
                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "Customer created successfully!";
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Error saving customer: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            await LoadCustomers();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteCustomerAsync(int customerId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI");
                var token = HttpContext.Session.GetString("AuthToken");
                
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.DeleteAsync($"api/customers/{customerId}");

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Customer deleted successfully!";
                }
                else
                {
                    ErrorMessage = $"Error deleting customer: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            await LoadCustomers();
            return Page();
        }

        private async Task LoadCustomers()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI");
                var token = HttpContext.Session.GetString("AuthToken");
                
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync("api/customers");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Customers = JsonSerializer.Deserialize<List<CustomerDto>>(content, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = "Unauthorized: Please log in to access customers.";
                }
                else
                {
                    ErrorMessage = "Unable to load customers. Please try again later.";
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

    public class CustomerDto
    {
        public int CustomerID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}