using Taskify.ApiService.Models;

namespace Taskify.ApiService.Services;

public interface ITaskService
{
    Task<TaskItem> CreateTaskAsync(string title, string? description, string status, string priority, DateTime? dueDate, Guid? assigneeId, Guid projectId, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItem>> GetTasksByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<TaskItem> UpdateTaskAsync(Guid taskId, string? title, string? description, string? status, string? priority, DateTime? dueDate, Guid? assigneeId, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
}

public class TaskService : ITaskService
{
    private readonly TaskifyDbContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(TaskifyDbContext context, ILogger<TaskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<TaskItem> CreateTaskAsync(string title, string? description, string status, string priority, DateTime? dueDate, Guid? assigneeId, Guid projectId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<TaskItem?> GetTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<IEnumerable<TaskItem>> GetTasksByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<TaskItem> UpdateTaskAsync(Guid taskId, string? title, string? description, string? status, string? priority, DateTime? dueDate, Guid? assigneeId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<bool> DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }
}