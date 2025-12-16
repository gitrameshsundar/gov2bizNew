using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json;
using LicenseManagement.DTO;
namespace LicenseManagement.Service
{
    public class ClientAPIService : IClientAPIService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;

        // Use Dependency Injection to get HttpClient and Configuration
        public ClientAPIService(IHttpClientFactory httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<UserDto?> LoginAsync(LoginView loginInput)
        {
            
                var client = _httpClient.CreateClient("GatewayAPI");
                var response = await client.GetAsync($"api/usersauth/{loginInput.Username}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                else
                {
                    // Parse API response
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ApiResult<UserDto>>(jsonContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResult?.Data;
                }
            }
        }
    }

