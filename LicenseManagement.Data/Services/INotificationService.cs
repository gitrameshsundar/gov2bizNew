using LicenseManagement.Data.Models;

namespace LicenseManagement.Data.Services
{
    public interface INotificationService
    {
        Task<List<Notification>> GetAllNotificationsAsync();
        Task<Notification?> GetNotificationByIdAsync(int id);
        Task<List<Notification>> GetNotificationsByStatusAsync(string status);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<Notification> UpdateNotificationAsync(int id, Notification notification);
        Task DeleteNotificationAsync(int id);
    }
}