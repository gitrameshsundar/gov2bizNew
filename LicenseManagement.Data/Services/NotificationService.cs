using LicenseManagement.Data.Models;
using LicenseManagement.Data.Repositories;

namespace LicenseManagement.Data.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Notification>> GetAllNotificationsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Notification?> GetNotificationByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid notification ID");

            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<Notification>> GetNotificationsByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status is required");

            return await _repository.GetByStatusAsync(status);
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            if (string.IsNullOrWhiteSpace(notification.Title))
                throw new ArgumentException("Notification title is required");

            if (string.IsNullOrWhiteSpace(notification.Message))
                throw new ArgumentException("Notification message is required");

            return await _repository.CreateAsync(notification);
        }

        public async Task<Notification> UpdateNotificationAsync(int id, Notification notification)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid notification ID");

            if (string.IsNullOrWhiteSpace(notification.Title))
                throw new ArgumentException("Notification title is required");

            return await _repository.UpdateAsync(id, notification);
        }

        public async Task DeleteNotificationAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid notification ID");

            await _repository.DeleteAsync(id);
        }
    }
}