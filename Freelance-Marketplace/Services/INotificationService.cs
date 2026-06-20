using FreelanceMarketplace.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreelanceMarketplace.Services
{
    public interface INotificationService
    {
        Task CreateAsync(string userId, string message, string? link = null, string? type = null);
        Task<int> GetUnreadCountAsync(string userId);
        Task<List<Notification>> GetLatestAsync(string userId, int count = 5);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}