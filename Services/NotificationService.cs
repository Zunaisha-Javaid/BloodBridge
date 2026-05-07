using BloodBridge.Data;
using BloodBridge.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.Services
{
    public interface INotificationService
    {
        Task CreateAsync(string userId, string message, string? link = null);
        Task<List<Notification>> GetUnreadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAllReadAsync(string userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(string userId, string message, string? link = null)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Message = message,
                Link = link,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUnreadAsync(string userId)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAllReadAsync(string userId)
        {
            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            await _db.SaveChangesAsync();
        }
    }
}
