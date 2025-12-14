using LicenseManagement.Data.Data;
using LicenseManagement.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseManagement.Data.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetAllAsync()
        {
            return await _context.Notifications.ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationID == id);
        }

        public async Task<List<Notification>> GetByStatusAsync(string status)
        {
            return await _context.Notifications.Where(n => n.Status == status).ToListAsync();
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification> UpdateAsync(int id, Notification notification)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Notification {id} not found");

            existing.Title = notification.Title;
            existing.Message = notification.Message;
            existing.Status = notification.Status;
            existing.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var notification = await GetByIdAsync(id);
            if (notification == null)
                throw new KeyNotFoundException($"Notification {id} not found");

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }
}