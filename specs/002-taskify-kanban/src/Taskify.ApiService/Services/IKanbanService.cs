using Taskify.ApiService.Models;

namespace Taskify.ApiService.Services;

public interface IKanbanService
{
    Task<KanbanBoard> CreateBoardAsync(Guid projectId, string name, string? description, KanbanBoardSettings settings, CancellationToken cancellationToken = default);
    Task<KanbanBoard?> GetBoardAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<IEnumerable<KanbanBoard>> GetBoardsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<KanbanBoard> UpdateBoardAsync(Guid boardId, string? name, string? description, KanbanBoardSettings? settings, CancellationToken cancellationToken = default);
    Task<bool> DeleteBoardAsync(Guid boardId, CancellationToken cancellationToken = default);
    
    Task<BoardColumn> CreateColumnAsync(Guid boardId, string name, string keyField, int position, int? wipLimit, string? color, CancellationToken cancellationToken = default);
    Task<BoardColumn> UpdateColumnAsync(Guid columnId, string? name, string? keyField, int? position, int? wipLimit, string? color, CancellationToken cancellationToken = default);
    Task<bool> DeleteColumnAsync(Guid columnId, CancellationToken cancellationToken = default);
    
    Task<TaskPosition> MoveTaskAsync(Guid taskId, Guid boardId, Guid newColumnId, decimal newPosition, string? newSwimlaneValue, int lastKnownVersion, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskPosition> AssignTaskToBoardAsync(Guid taskId, Guid boardId, Guid columnId, decimal? position, string? swimlaneValue, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> RemoveTaskFromBoardAsync(Guid taskId, Guid boardId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<TaskPosition>> GetTaskPositionsAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<TaskItem> Tasks, IEnumerable<BoardColumn> Columns, IEnumerable<string> Swimlanes)> GetBoardTasksAsync(
        Guid boardId, 
        BoardFilterCriteria? filterCriteria = null, 
        int page = 0, 
        int pageSize = 100, 
        CancellationToken cancellationToken = default);
}

public class KanbanService : IKanbanService
{
    private readonly TaskifyDbContext _context;
    private readonly ILogger<KanbanService> _logger;

    public KanbanService(TaskifyDbContext context, ILogger<KanbanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<KanbanBoard> CreateBoardAsync(Guid projectId, string name, string? description, KanbanBoardSettings settings, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<KanbanBoard?> GetBoardAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<IEnumerable<KanbanBoard>> GetBoardsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<KanbanBoard> UpdateBoardAsync(Guid boardId, string? name, string? description, KanbanBoardSettings? settings, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<bool> DeleteBoardAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<BoardColumn> CreateColumnAsync(Guid boardId, string name, string keyField, int position, int? wipLimit, string? color, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<BoardColumn> UpdateColumnAsync(Guid columnId, string? name, string? keyField, int? position, int? wipLimit, string? color, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<bool> DeleteColumnAsync(Guid columnId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<TaskPosition> MoveTaskAsync(Guid taskId, Guid boardId, Guid newColumnId, decimal newPosition, string? newSwimlaneValue, int lastKnownVersion, Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<TaskPosition> AssignTaskToBoardAsync(Guid taskId, Guid boardId, Guid columnId, decimal? position, string? swimlaneValue, Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<bool> RemoveTaskFromBoardAsync(Guid taskId, Guid boardId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<IEnumerable<TaskPosition>> GetTaskPositionsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }

    public Task<(IEnumerable<TaskItem> Tasks, IEnumerable<BoardColumn> Columns, IEnumerable<string> Swimlanes)> GetBoardTasksAsync(Guid boardId, BoardFilterCriteria? filterCriteria = null, int page = 0, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in Phase 1.2");
    }
}