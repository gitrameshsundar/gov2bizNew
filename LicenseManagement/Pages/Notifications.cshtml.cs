using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LicenseManagement.DTO;
namespace LicenseManagement.Pages
{
    public class NotificationsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public List<NotificationDto> Notifications { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        public NotificationsModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            if (PageNumber < 1)
                PageNumber = 1;

            await LoadNotifications();
        }

        public async Task<IActionResult> OnPostSaveNotificationAsync(int notificationId, string title, string message, string status)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                ErrorMessage = "Notification title is required.";
                await LoadNotifications();
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("GatewayAPI");
                client.BaseAddress = new Uri(_configuration["APISettings:BaseUrl"]);
                var notification = new { title, message, status };
                var content = new StringContent(JsonSerializer.Serialize(notification), Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                
                if (notificationId > 0)
                {
                    response = await client.PutAsync($"api/notifications/{notificationId}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "Notification updated successfully!";
                    }
                }
                else
                {
                    response = await client.PostAsync("api/notifications", content);
                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "Notification created successfully!";
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Error saving notification: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            PageNumber = 1;
            await LoadNotifications();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteNotificationAsync(int notificationId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI");
                client.BaseAddress = new Uri(_configuration["APISettings:BaseUrl"]);
                var response = await client.DeleteAsync($"api/notifications/{notificationId}");

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Notification deleted successfully!";
                }
                else
                {
                    ErrorMessage = $"Error deleting notification: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            await LoadNotifications();
            return Page();
        }

        private async Task LoadNotifications()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CustomerAPI");
                client.BaseAddress = new Uri(_configuration["APISettings:BaseUrl"]);
                var response = await client.GetAsync("api/notifications");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResult<List<NotificationDto>>>(jsonContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var allNotifications = result?.Data ?? new();
                    TotalCount = allNotifications.Count;
                    TotalPages = Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

                    if (PageNumber > TotalPages)
                        PageNumber = TotalPages;

                    HasPreviousPage = PageNumber > 1;
                    HasNextPage = PageNumber < TotalPages;

                    Notifications = allNotifications
                        .Skip((PageNumber - 1) * PageSize)
                        .Take(PageSize)
                        .ToList();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = "Unauthorized: Please log in to access notifications.";
                }
                else
                {
                    ErrorMessage = "Unable to load notifications. Please try again later.";
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