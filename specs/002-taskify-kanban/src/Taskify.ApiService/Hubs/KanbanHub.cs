using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Taskify.ApiService.Services;
using System.Security.Claims;

namespace Taskify.ApiService.Hubs;

[Authorize]
public class KanbanHub : Hub
{
    private readonly IKanbanService _kanbanService;
    private readonly ILogger<KanbanHub> _logger;

    public KanbanHub(IKanbanService kanbanService, ILogger<KanbanHub> logger)
    {
        _kanbanService = kanbanService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        var connectionId = Context.ConnectionId;
        
        _logger.LogInformation("User {UserId} connected to kanban hub with connection {ConnectionId}", userId, connectionId);
        
        // Send connection confirmation
        await Clients.Caller.SendAsync("ConnectionEstablished", new
        {
            ConnectionId = connectionId,
            UserId = userId,
            ConnectedAt = DateTime.UtcNow
        });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        var connectionId = Context.ConnectionId;
        
        _logger.LogInformation("User {UserId} disconnected from kanban hub (connection {ConnectionId})", userId, connectionId);
        
        if (exception != null)
        {
            _logger.LogError(exception, "User {UserId} disconnected due to error", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<bool> JoinBoard(string boardId)
    {
        try
        {
            if (!Guid.TryParse(boardId, out var parsedBoardId))
            {
                throw new HubException("Invalid board ID format");
            }

            var userId = GetCurrentUserId();
            
            // TODO: Verify user has access to this board
            // For now, allowing all authenticated users
            
            await Groups.AddToGroupAsync(Context.ConnectionId, $"board_{boardId}");
            
            _logger.LogInformation("User {UserId} joined board {BoardId}", userId, boardId);
            
            // Notify other users in the board
            await Clients.OthersInGroup($"board_{boardId}").SendAsync("UserPresenceChanged", new
            {
                UserId = userId,
                BoardId = parsedBoardId,
                Status = "online",
                Timestamp = DateTime.UtcNow
            });
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining board {BoardId} for user {UserId}", boardId, GetCurrentUserId());
            throw new HubException($"Failed to join board: {ex.Message}");
        }
    }

    public async Task<bool> LeaveBoard(string boardId)
    {
        try
        {
            if (!Guid.TryParse(boardId, out var parsedBoardId))
            {
                throw new HubException("Invalid board ID format");
            }

            var userId = GetCurrentUserId();
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board_{boardId}");
            
            _logger.LogInformation("User {UserId} left board {BoardId}", userId, boardId);
            
            // Notify other users in the board
            await Clients.OthersInGroup($"board_{boardId}").SendAsync("UserPresenceChanged", new
            {
                UserId = userId,
                BoardId = parsedBoardId,
                Status = "offline",
                Timestamp = DateTime.UtcNow
            });
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving board {BoardId} for user {UserId}", boardId, GetCurrentUserId());
            throw new HubException($"Failed to leave board: {ex.Message}");
        }
    }

    public async Task StartTaskDrag(string taskId, string boardId, string currentColumnId)
    {
        try
        {
            ValidateGuids(taskId, boardId, currentColumnId);
            
            var userId = GetCurrentUserId();
            
            await Clients.OthersInGroup($"board_{boardId}").SendAsync("TaskDragStateChanged", new
            {
                TaskId = Guid.Parse(taskId),
                BoardId = Guid.Parse(boardId),
                DragState = "start",
                CurrentColumnId = Guid.Parse(currentColumnId),
                DraggedBy = userId,
                DraggedByName = GetCurrentUserName(),
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogDebug("Task drag started: {TaskId} by user {UserId}", taskId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting task drag for task {TaskId}", taskId);
            throw new HubException($"Failed to start task drag: {ex.Message}");
        }
    }

    public async Task UpdateTaskDrag(string taskId, string boardId, string? targetColumnId)
    {
        try
        {
            ValidateGuids(taskId, boardId);
            if (targetColumnId != null && !Guid.TryParse(targetColumnId, out _))
            {
                throw new HubException("Invalid target column ID format");
            }
            
            var userId = GetCurrentUserId();
            
            await Clients.OthersInGroup($"board_{boardId}").SendAsync("TaskDragStateChanged", new
            {
                TaskId = Guid.Parse(taskId),
                BoardId = Guid.Parse(boardId),
                DragState = "move",
                TargetColumnId = targetColumnId != null ? Guid.Parse(targetColumnId) : (Guid?)null,
                DraggedBy = userId,
                DraggedByName = GetCurrentUserName(),
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task drag for task {TaskId}", taskId);
            throw new HubException($"Failed to update task drag: {ex.Message}");
        }
    }

    public async Task EndTaskDrag(string taskId, string boardId, string? finalColumnId)
    {
        try
        {
            ValidateGuids(taskId, boardId);
            if (finalColumnId != null && !Guid.TryParse(finalColumnId, out _))
            {
                throw new HubException("Invalid final column ID format");
            }
            
            var userId = GetCurrentUserId();
            
            await Clients.OthersInGroup($"board_{boardId}").SendAsync("TaskDragStateChanged", new
            {
                TaskId = Guid.Parse(taskId),
                BoardId = Guid.Parse(boardId),
                DragState = "end",
                TargetColumnId = finalColumnId != null ? Guid.Parse(finalColumnId) : (Guid?)null,
                DraggedBy = userId,
                DraggedByName = GetCurrentUserName(),
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogDebug("Task drag ended: {TaskId} by user {UserId}", taskId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending task drag for task {TaskId}", taskId);
            throw new HubException($"Failed to end task drag: {ex.Message}");
        }
    }

    public async Task<object> MoveTask(object moveRequest)
    {
        try
        {
            // TODO: Implement actual task move logic in Phase 1.4
            // For now, return a placeholder response
            
            _logger.LogWarning("MoveTask called but not yet implemented - will be completed in Phase 1.4");
            
            return new
            {
                Success = false,
                Error = "MoveTask not yet implemented - will be completed in Phase 1.4",
                ConflictDetected = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving task via SignalR");
            throw new HubException($"Failed to move task: {ex.Message}");
        }
    }

    public async Task UpdatePresence(string boardId, string status)
    {
        try
        {
            if (!Guid.TryParse(boardId, out var parsedBoardId))
            {
                throw new HubException("Invalid board ID format");
            }

            var validStatuses = new[] { "active", "idle" };
            if (!validStatuses.Contains(status))
            {
                throw new HubException("Invalid status. Must be 'active' or 'idle'");
            }
            
            var userId = GetCurrentUserId();
            
            await Clients.OthersInGroup($"board_{boardId}").SendAsync("UserPresenceChanged", new
            {
                UserId = userId,
                UserName = GetCurrentUserName(),
                BoardId = parsedBoardId,
                Status = status,
                LastActivity = DateTime.UtcNow,
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogDebug("User {UserId} updated presence to {Status} on board {BoardId}", userId, status, boardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating presence for user {UserId}", GetCurrentUserId());
            throw new HubException($"Failed to update presence: {ex.Message}");
        }
    }

    public async Task<object> RequestBoardSync(string boardId)
    {
        try
        {
            if (!Guid.TryParse(boardId, out var parsedBoardId))
            {
                throw new HubException("Invalid board ID format");
            }
            
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("Board sync requested by user {UserId} for board {BoardId}", userId, boardId);
            
            // TODO: Implement actual sync logic in Phase 1.4
            
            return new
            {
                Timestamp = DateTime.UtcNow,
                SyncRequested = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting board sync for board {BoardId}", boardId);
            throw new HubException($"Failed to request board sync: {ex.Message}");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? Context.User?.FindFirst("sub")?.Value;
        
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new HubException("User ID not found in claims");
        }
        
        return userId;
    }

    private string GetCurrentUserName()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value 
               ?? Context.User?.FindFirst("name")?.Value 
               ?? "Unknown User";
    }

    private static void ValidateGuids(params string[] guidStrings)
    {
        foreach (var guidString in guidStrings)
        {
            if (!Guid.TryParse(guidString, out _))
            {
                throw new HubException($"Invalid GUID format: {guidString}");
            }
        }
    }
}