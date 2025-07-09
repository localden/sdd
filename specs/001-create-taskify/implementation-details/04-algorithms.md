# Algorithms: Drag-Drop and Real-Time Logic

## Drag-and-Drop State Management

### Client-Side Drag Algorithm
```csharp
public class DragDropHandler
{
    private TaskItem _draggedItem;
    private TaskStatus _originalStatus;
    private bool _isDragging;
    
    public async Task OnDragStart(TaskItem item)
    {
        _draggedItem = item;
        _originalStatus = item.Status;
        _isDragging = true;
        
        // Visual feedback
        await UpdateTaskVisualState(item, DragState.Dragging);
    }
    
    public async Task OnDragOver(TaskStatus targetStatus)
    {
        if (!_isDragging || _draggedItem == null) return;
        
        // Optimistic UI update
        await UpdateTaskStatus(_draggedItem, targetStatus, optimistic: true);
    }
    
    public async Task OnDrop(TaskStatus targetStatus)
    {
        if (!_isDragging || _draggedItem == null) return;
        
        try
        {
            // Server synchronization
            var result = await _taskService.UpdateTaskStatusAsync(_draggedItem.Id, targetStatus);
            
            if (result.Success)
            {
                // Confirm optimistic update
                await UpdateTaskStatus(_draggedItem, targetStatus, optimistic: false);
                await NotifyTaskStatusChanged(_draggedItem.Id, targetStatus);
            }
            else
            {
                // Rollback on failure
                await RollbackTaskStatus(_draggedItem, _originalStatus);
                await ShowErrorMessage(result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            // Network error - rollback
            await RollbackTaskStatus(_draggedItem, _originalStatus);
            await ShowErrorMessage("Network error occurred. Please try again.");
        }
        finally
        {
            _isDragging = false;
            _draggedItem = null;
        }
    }
    
    private async Task UpdateTaskStatus(TaskItem item, TaskStatus status, bool optimistic)
    {
        item.Status = status;
        item.LastModified = DateTime.UtcNow;
        
        if (!optimistic)
        {
            // Persist to local state
            await _stateManager.UpdateTaskAsync(item);
        }
        
        // Update UI
        await InvokeAsync(StateHasChanged);
    }
    
    private async Task RollbackTaskStatus(TaskItem item, TaskStatus originalStatus)
    {
        item.Status = originalStatus;
        await UpdateTaskVisualState(item, DragState.Error);
        await InvokeAsync(StateHasChanged);
        
        // Remove error state after delay
        await Task.Delay(2000);
        await UpdateTaskVisualState(item, DragState.Normal);
    }
}
```

### Server-Side Validation Algorithm
```csharp
public class TaskStatusValidator
{
    public ValidationResult ValidateStatusTransition(TaskStatus from, TaskStatus to)
    {
        // Define valid transitions
        var validTransitions = new Dictionary<TaskStatus, TaskStatus[]>
        {
            { TaskStatus.ToDo, new[] { TaskStatus.InProgress } },
            { TaskStatus.InProgress, new[] { TaskStatus.ToDo, TaskStatus.InReview } },
            { TaskStatus.InReview, new[] { TaskStatus.InProgress, TaskStatus.Done } },
            { TaskStatus.Done, new[] { TaskStatus.InReview } }
        };
        
        if (!validTransitions.ContainsKey(from))
        {
            return ValidationResult.Invalid($"Invalid source status: {from}");
        }
        
        if (!validTransitions[from].Contains(to))
        {
            return ValidationResult.Invalid($"Invalid transition from {from} to {to}");
        }
        
        return ValidationResult.Valid();
    }
    
    public async Task<ValidationResult> ValidateBusinessRules(Task task, TaskStatus newStatus)
    {
        // Business rule: Tasks in Review must have assignee
        if (newStatus == TaskStatus.InReview && task.AssignedUserId == null)
        {
            return ValidationResult.Invalid("Tasks must be assigned before moving to Review");
        }
        
        // Business rule: Done tasks must have at least one comment
        if (newStatus == TaskStatus.Done)
        {
            var commentCount = await _commentService.GetCommentCountAsync(task.Id);
            if (commentCount == 0)
            {
                return ValidationResult.Invalid("Tasks must have at least one comment before completion");
            }
        }
        
        return ValidationResult.Valid();
    }
}
```

## Real-Time Synchronization

### SignalR Hub Algorithm
```csharp
public class NotificationHub : Hub
{
    private readonly ITaskService _taskService;
    private readonly ILogger<NotificationHub> _logger;
    
    public async Task JoinProject(int projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Project_{projectId}");
        _logger.LogInformation($"User {Context.ConnectionId} joined Project_{projectId}");
    }
    
    public async Task LeaveProject(int projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Project_{projectId}");
        _logger.LogInformation($"User {Context.ConnectionId} left Project_{projectId}");
    }
    
    public async Task NotifyTaskStatusChanged(int taskId, TaskStatus newStatus, int projectId)
    {
        var notification = new TaskStatusChangedNotification
        {
            TaskId = taskId,
            NewStatus = newStatus.ToString(),
            ProjectId = projectId,
            Timestamp = DateTime.UtcNow
        };
        
        // Notify all users in the project
        await Clients.Group($"Project_{projectId}").SendAsync("TaskStatusChanged", notification);
        
        // Update task counts for project dashboard
        await UpdateProjectTaskCounts(projectId);
    }
    
    private async Task UpdateProjectTaskCounts(int projectId)
    {
        var taskCounts = await _taskService.GetTaskCountsByProjectAsync(projectId);
        
        var countUpdate = new ProjectTaskCountsNotification
        {
            ProjectId = projectId,
            TaskCounts = taskCounts,
            Timestamp = DateTime.UtcNow
        };
        
        await Clients.Group($"Project_{projectId}").SendAsync("ProjectTaskCountsUpdated", countUpdate);
    }
}
```

### Connection Management Algorithm
```csharp
public class SignalRConnectionManager
{
    private readonly HubConnection _connection;
    private readonly ILogger<SignalRConnectionManager> _logger;
    private readonly Timer _heartbeatTimer;
    private bool _isConnected;
    
    public async Task<bool> ConnectAsync()
    {
        try
        {
            await _connection.StartAsync();
            _isConnected = true;
            
            // Start heartbeat
            _heartbeatTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(30));
            
            _logger.LogInformation("SignalR connection established");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to SignalR hub");
            return false;
        }
    }
    
    public async Task<bool> ReconnectAsync()
    {
        const int maxRetries = 5;
        const int baseDelayMs = 1000;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await _connection.StartAsync();
                _isConnected = true;
                
                _logger.LogInformation($"SignalR reconnection successful (attempt {attempt})");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Reconnection attempt {attempt} failed");
                
                if (attempt < maxRetries)
                {
                    var delay = TimeSpan.FromMilliseconds(baseDelayMs * Math.Pow(2, attempt - 1));
                    await Task.Delay(delay);
                }
            }
        }
        
        _logger.LogError("All reconnection attempts failed");
        return false;
    }
    
    private async void HeartbeatCallback(object state)
    {
        if (_connection.State != HubConnectionState.Connected)
        {
            _isConnected = false;
            await ReconnectAsync();
        }
    }
}
```

## Concurrency Control

### Optimistic Locking Algorithm
```csharp
public class OptimisticLockManager
{
    private readonly Dictionary<int, TaskVersion> _taskVersions = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public async Task<UpdateResult> UpdateTaskWithLockAsync(int taskId, TaskStatus newStatus, int expectedVersion)
    {
        await _semaphore.WaitAsync();
        
        try
        {
            var currentVersion = await GetTaskVersionAsync(taskId);
            
            if (currentVersion != expectedVersion)
            {
                return UpdateResult.Conflict($"Task was modified by another user. Expected version {expectedVersion}, current version {currentVersion}");
            }
            
            // Perform update
            var result = await UpdateTaskInDatabaseAsync(taskId, newStatus);
            
            if (result.Success)
            {
                // Increment version
                _taskVersions[taskId] = new TaskVersion(currentVersion + 1, DateTime.UtcNow);
            }
            
            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private async Task<int> GetTaskVersionAsync(int taskId)
    {
        if (!_taskVersions.ContainsKey(taskId))
        {
            var version = await _database.GetTaskVersionAsync(taskId);
            _taskVersions[taskId] = new TaskVersion(version, DateTime.UtcNow);
        }
        
        return _taskVersions[taskId].Version;
    }
}
```

### Conflict Resolution Algorithm
```csharp
public class ConflictResolver
{
    public async Task<ConflictResolution> ResolveTaskStatusConflict(
        TaskStatusConflict conflict)
    {
        // Last-writer-wins strategy
        if (conflict.Timestamp1 > conflict.Timestamp2)
        {
            return ConflictResolution.UseFirst(conflict.Status1);
        }
        else if (conflict.Timestamp2 > conflict.Timestamp1)
        {
            return ConflictResolution.UseSecond(conflict.Status2);
        }
        else
        {
            // Same timestamp - use status priority
            return ResolveByStatusPriority(conflict.Status1, conflict.Status2);
        }
    }
    
    private ConflictResolution ResolveByStatusPriority(TaskStatus status1, TaskStatus status2)
    {
        var priorityMap = new Dictionary<TaskStatus, int>
        {
            { TaskStatus.Done, 4 },
            { TaskStatus.InReview, 3 },
            { TaskStatus.InProgress, 2 },
            { TaskStatus.ToDo, 1 }
        };
        
        var priority1 = priorityMap[status1];
        var priority2 = priorityMap[status2];
        
        return priority1 >= priority2 
            ? ConflictResolution.UseFirst(status1) 
            : ConflictResolution.UseSecond(status2);
    }
}
```

## Performance Optimization

### Debouncing Algorithm
```csharp
public class DebounceService
{
    private readonly Dictionary<string, CancellationTokenSource> _debounceTasks = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public async Task DebounceAsync(string key, Func<Task> action, TimeSpan delay)
    {
        await _semaphore.WaitAsync();
        
        try
        {
            // Cancel existing debounce task for this key
            if (_debounceTasks.ContainsKey(key))
            {
                _debounceTasks[key].Cancel();
            }
            
            // Create new cancellation token
            var cts = new CancellationTokenSource();
            _debounceTasks[key] = cts;
            
            // Schedule debounced action
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, cts.Token);
                    
                    if (!cts.Token.IsCancellationRequested)
                    {
                        await action();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when debounce is cancelled
                }
                finally
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        _debounceTasks.Remove(key);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            });
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

### Caching Algorithm
```csharp
public class TaskCacheManager
{
    private readonly MemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
    
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory)
    {
        if (_cache.TryGetValue(key, out T cachedValue))
        {
            return cachedValue;
        }
        
        var value = await factory();
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration,
            Priority = CacheItemPriority.Normal
        };
        
        _cache.Set(key, value, cacheOptions);
        return value;
    }
    
    public void InvalidateTaskCache(int taskId)
    {
        var keysToRemove = new[]
        {
            $"task_{taskId}",
            $"task_comments_{taskId}",
            $"project_tasks_{GetProjectIdForTask(taskId)}"
        };
        
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }
    }
}
```

## Error Handling Algorithms

### Retry Logic with Exponential Backoff
```csharp
public class RetryService
{
    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        TimeSpan baseDelay = default)
    {
        if (baseDelay == default)
            baseDelay = TimeSpan.FromSeconds(1);
        
        Exception lastException = null;
        
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (IsRetryableException(ex))
            {
                lastException = ex;
                
                if (attempt < maxRetries)
                {
                    var delay = TimeSpan.FromMilliseconds(
                        baseDelay.TotalMilliseconds * Math.Pow(2, attempt));
                    
                    await Task.Delay(delay);
                }
            }
        }
        
        throw new RetryExhaustedException(
            $"Operation failed after {maxRetries + 1} attempts", 
            lastException);
    }
    
    private bool IsRetryableException(Exception ex)
    {
        return ex is HttpRequestException ||
               ex is TaskCanceledException ||
               ex is SocketException ||
               (ex is SqlException sqlEx && IsTransientSqlError(sqlEx));
    }
}
```

### Circuit Breaker Algorithm
```csharp
public class CircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private CircuitState _state = CircuitState.Closed;
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _timeout)
            {
                _state = CircuitState.HalfOpen;
            }
            else
            {
                throw new CircuitBreakerOpenException("Circuit breaker is open");
            }
        }
        
        try
        {
            var result = await operation();
            
            if (_state == CircuitState.HalfOpen)
            {
                _state = CircuitState.Closed;
                _failureCount = 0;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            
            if (_failureCount >= _failureThreshold)
            {
                _state = CircuitState.Open;
            }
            
            throw;
        }
    }
}

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}
```

## Data Structures

### Task State Machine
```csharp
public class TaskStateMachine
{
    private readonly Dictionary<TaskStatus, TaskStatus[]> _validTransitions = new()
    {
        { TaskStatus.ToDo, new[] { TaskStatus.InProgress } },
        { TaskStatus.InProgress, new[] { TaskStatus.ToDo, TaskStatus.InReview } },
        { TaskStatus.InReview, new[] { TaskStatus.InProgress, TaskStatus.Done } },
        { TaskStatus.Done, new[] { TaskStatus.InReview } }
    };
    
    public bool CanTransition(TaskStatus from, TaskStatus to)
    {
        return _validTransitions.ContainsKey(from) && 
               _validTransitions[from].Contains(to);
    }
    
    public TaskStatus[] GetValidTransitions(TaskStatus currentStatus)
    {
        return _validTransitions.ContainsKey(currentStatus) 
            ? _validTransitions[currentStatus] 
            : Array.Empty<TaskStatus>();
    }
}
```

### Priority Queue for Notifications
```csharp
public class NotificationQueue
{
    private readonly PriorityQueue<NotificationItem, int> _queue = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public async Task EnqueueAsync(NotificationItem item, NotificationPriority priority)
    {
        await _semaphore.WaitAsync();
        try
        {
            _queue.Enqueue(item, (int)priority);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<NotificationItem> DequeueAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return _queue.Count > 0 ? _queue.Dequeue() : null;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

public enum NotificationPriority
{
    Low = 3,
    Normal = 2,
    High = 1,
    Critical = 0
}
```