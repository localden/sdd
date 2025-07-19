using Taskify.ApiService.Models;

namespace Taskify.ApiService.Services;

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(Guid userId, string type, string title, string message, Guid? relatedTaskId = null, Guid? relatedBoardId = null, string? actionUrl = null, string priority = "normal", CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, int page = 0, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class NotificationService : INotificationService
{
    private readonly TaskifyDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(TaskifyDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<Notification> CreateNotificationAsync(Guid userId, string type, string title, string message, Guid? relatedTaskId = null, Guid? relatedBoardId = null, string? actionUrl = null, string priority = "normal", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, int page = 0, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<bool> DeleteNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }
}