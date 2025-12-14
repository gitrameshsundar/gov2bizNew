using LicenseManagement.Data.Models;

namespace LicenseManagement.Data.Repositories
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetAllAsync();
        Task<Notification?> GetByIdAsync(int id);
        Task<List<Notification>> GetByStatusAsync(string status);
        Task<Notification> CreateAsync(Notification notification);
        Task<Notification> UpdateAsync(int id, Notification notification);
        Task DeleteAsync(int id);
    }
}