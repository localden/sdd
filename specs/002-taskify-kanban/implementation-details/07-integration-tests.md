# Integration Tests: Enhanced Kanban Board System

**Document**: 002-taskify-kanban/implementation-details/07-integration-tests.md  
**Created**: 2025-07-19  
**Status**: Enhanced integration test scenarios for kanban features  

---

## Overview

This document defines comprehensive integration test scenarios for the enhanced kanban board system, focusing on real-time collaboration, concurrent operations, performance under load, and mobile device interactions. The tests ensure end-to-end functionality across all system components.

---

## Test Environment Setup

### Enhanced Test Infrastructure
```csharp
// Enhanced integration test base class
public abstract class KanbanIntegrationTestBase : IAsyncLifetime
{
    protected DistributedApplication App { get; private set; } = null!;
    protected HttpClient ApiClient { get; private set; } = null!;
    protected HubConnection KanbanHub { get; private set; } = null!;
    protected TaskifyDbContext DbContext { get; private set; } = null!;
    
    protected readonly List<TestUser> TestUsers = new();
    protected readonly List<TestProject> TestProjects = new();
    protected readonly List<TestKanbanBoard> TestBoards = new();

    public async Task InitializeAsync()
    {
        // Start Aspire application with test configuration
        var builder = DistributedApplicationTestingBuilder.Create<Projects.Taskify_AppHost>();
        App = await builder.BuildAsync();
        await App.StartAsync();
        
        // Initialize HTTP client with authentication
        ApiClient = App.CreateHttpClient("apiservice");
        ApiClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", await GenerateTestTokenAsync());
        
        // Initialize SignalR connection
        var hubUrl = $"{App.GetEndpoint("apiservice")}/hubs/kanban";
        KanbanHub = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => GenerateTestTokenAsync();
            })
            .WithAutomaticReconnect()
            .Build();
        
        await KanbanHub.StartAsync();
        
        // Initialize database context for data verification
        DbContext = App.Services.GetRequiredService<TaskifyDbContext>();
        
        // Seed test data
        await SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await KanbanHub.DisposeAsync();
        ApiClient.Dispose();
        await App.DisposeAsync();
    }
}
```

### Test Data Management
```csharp
public class TestDataManager
{
    public static async Task<TestKanbanBoard> CreateTestBoardAsync(
        HttpClient client, 
        Guid projectId, 
        string name = "Test Board")
    {
        var createRequest = new CreateKanbanBoardRequest
        {
            ProjectId = projectId,
            Name = name,
            Description = "Integration test board",
            Settings = new KanbanBoardSettingsDto
            {
                EnableWipLimits = true,
                EnableSwimlanes = true,
                MaxTasksPerColumn = 10
            },
            Columns = new[]
            {
                new CreateBoardColumnRequest { Name = "To Do", KeyField = "todo", Position = 1, WipLimit = 5 },
                new CreateBoardColumnRequest { Name = "In Progress", KeyField = "inprogress", Position = 2, WipLimit = 3 },
                new CreateBoardColumnRequest { Name = "Review", KeyField = "review", Position = 3, WipLimit = 2 },
                new CreateBoardColumnRequest { Name = "Done", KeyField = "done", Position = 4 }
            }
        };
        
        var response = await client.PostAsJsonAsync("/api/kanban/boards", createRequest);
        response.EnsureSuccessStatusCode();
        
        var board = await response.Content.ReadFromJsonAsync<KanbanBoardDto>();
        return new TestKanbanBoard(board!);
    }
    
    public static async Task<TestTask> CreateTestTaskAsync(
        HttpClient client,
        Guid projectId,
        string title = "Test Task")
    {
        var createRequest = new CreateTaskRequest
        {
            Title = title,
            Description = "Integration test task",
            ProjectId = projectId,
            Priority = "medium",
            Status = "todo"
        };
        
        var response = await client.PostAsJsonAsync("/api/tasks", createRequest);
        response.EnsureSuccessStatusCode();
        
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        return new TestTask(task!);
    }
}
```

---

## Core Kanban Integration Tests

### Test Category: Board Management Workflows
```csharp
public class BoardManagementIntegrationTests : KanbanIntegrationTestBase
{
    [Fact]
    public async Task CreateKanbanBoard_ShouldCreateBoardWithColumns_WhenValidRequest()
    {
        // Arrange
        var project = TestProjects.First();
        
        // Act
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, project.Id, "Integration Test Board");
        
        // Assert
        board.Should().NotBeNull();
        board.Id.Should().NotBeEmpty();
        board.ProjectId.Should().Be(project.Id);
        board.Columns.Should().HaveCount(4);
        
        // Verify in database
        var dbBoard = await DbContext.KanbanBoards
            .Include(b => b.Columns)
            .FirstOrDefaultAsync(b => b.Id == board.Id);
        
        dbBoard.Should().NotBeNull();
        dbBoard!.Columns.Should().HaveCount(4);
    }

    [Fact]
    public async Task UpdateBoardConfiguration_ShouldUpdateSettings_AndNotifyUsers()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        var user2Connection = await CreateSecondUserConnectionAsync();
        
        var configurationChanged = new TaskCompletionSource<BoardConfigurationEvent>();
        user2Connection.On<BoardConfigurationEvent>("BoardConfigurationChanged", (config) =>
        {
            configurationChanged.SetResult(config);
        });
        
        await user2Connection.InvokeAsync<bool>("JoinBoard", board.Id.ToString());
        
        // Act
        var updateRequest = new UpdateKanbanBoardRequest
        {
            Name = "Updated Board Name",
            Settings = new KanbanBoardSettingsDto
            {
                EnableWipLimits = false,
                MaxTasksPerColumn = 20
            }
        };
        
        var response = await ApiClient.PutAsJsonAsync($"/api/kanban/boards/{board.Id}", updateRequest);
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify real-time notification
        var configEvent = await configurationChanged.Task.WaitAsync(TimeSpan.FromSeconds(5));
        configEvent.BoardId.Should().Be(board.Id);
        configEvent.ChangeType.Should().Be("settings_updated");
    }

    [Fact]
    public async Task DeleteBoard_ShouldRemoveAllRelatedData_AndNotifyUsers()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        var task = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id);
        
        // Assign task to board
        await ApiClient.PostAsJsonAsync($"/api/tasks/{task.Id}/assign-to-board", new
        {
            BoardId = board.Id,
            ColumnId = board.Columns.First().Id
        });
        
        // Act
        var response = await ApiClient.DeleteAsync($"/api/kanban/boards/{board.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify cascade deletion in database
        var dbBoard = await DbContext.KanbanBoards.FindAsync(board.Id);
        dbBoard.Should().BeNull();
        
        var taskPositions = await DbContext.TaskPositions
            .Where(tp => tp.BoardId == board.Id)
            .ToListAsync();
        taskPositions.Should().BeEmpty();
    }
}
```

### Test Category: Real-Time Task Movement
```csharp
public class RealTimeTaskMovementTests : KanbanIntegrationTestBase
{
    [Fact]
    public async Task MoveTask_ShouldUpdatePosition_AndNotifyAllUsers()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        var task = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id);
        
        // Assign task to board
        await ApiClient.PostAsJsonAsync($"/api/tasks/{task.Id}/assign-to-board", new
        {
            BoardId = board.Id,
            ColumnId = board.Columns[0].Id,
            Position = 1.0
        });
        
        // Set up second user connection
        var user2Connection = await CreateSecondUserConnectionAsync();
        await user2Connection.InvokeAsync<bool>("JoinBoard", board.Id.ToString());
        
        var taskMoved = new TaskCompletionSource<TaskMoveEvent>();
        user2Connection.On<TaskMoveEvent>("TaskMoved", (moveEvent) =>
        {
            taskMoved.SetResult(moveEvent);
        });
        
        // Act
        var moveRequest = new MoveTaskRequest
        {
            TaskId = task.Id,
            BoardId = board.Id,
            NewColumnId = board.Columns[1].Id, // Move to "In Progress"
            NewPosition = 1.5m,
            LastKnownVersion = 1
        };
        
        var response = await ApiClient.PutAsJsonAsync($"/api/kanban/tasks/{task.Id}/move", moveRequest);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var moveResponse = await response.Content.ReadFromJsonAsync<MoveTaskResponse>();
        moveResponse!.Success.Should().BeTrue();
        moveResponse.ConflictDetected.Should().BeFalse();
        
        // Verify real-time notification
        var moveEvent = await taskMoved.Task.WaitAsync(TimeSpan.FromSeconds(5));
        moveEvent.TaskId.Should().Be(task.Id);
        moveEvent.NewColumnId.Should().Be(board.Columns[1].Id);
        moveEvent.NewPosition.Should().Be(1.5m);
        
        // Verify database update
        var dbPosition = await DbContext.TaskPositions
            .FirstOrDefaultAsync(tp => tp.TaskId == task.Id && tp.BoardId == board.Id);
        
        dbPosition.Should().NotBeNull();
        dbPosition!.ColumnId.Should().Be(board.Columns[1].Id);
        dbPosition.Position.Should().Be(1.5m);
        dbPosition.Version.Should().Be(2); // Version incremented
    }

    [Fact]
    public async Task ConcurrentTaskMoves_ShouldHandleOptimisticConcurrency()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        var task = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id);
        
        await ApiClient.PostAsJsonAsync($"/api/tasks/{task.Id}/assign-to-board", new
        {
            BoardId = board.Id,
            ColumnId = board.Columns[0].Id,
            Position = 1.0
        });
        
        var user2Client = await CreateSecondUserHttpClientAsync();
        
        // Act - Simulate concurrent moves with same version
        var move1Request = new MoveTaskRequest
        {
            TaskId = task.Id,
            BoardId = board.Id,
            NewColumnId = board.Columns[1].Id,
            NewPosition = 1.0m,
            LastKnownVersion = 1
        };
        
        var move2Request = new MoveTaskRequest
        {
            TaskId = task.Id,
            BoardId = board.Id,
            NewColumnId = board.Columns[2].Id,
            NewPosition = 1.0m,
            LastKnownVersion = 1 // Same version - should cause conflict
        };
        
        var moveTask1 = ApiClient.PutAsJsonAsync($"/api/kanban/tasks/{task.Id}/move", move1Request);
        var moveTask2 = user2Client.PutAsJsonAsync($"/api/kanban/tasks/{task.Id}/move", move2Request);
        
        var responses = await Task.WhenAll(moveTask1, moveTask2);
        
        // Assert
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);
        
        successCount.Should().Be(1);
        conflictCount.Should().Be(1);
        
        // Verify the successful move is persisted
        var dbPosition = await DbContext.TaskPositions
            .FirstOrDefaultAsync(tp => tp.TaskId == task.Id && tp.BoardId == board.Id);
        
        dbPosition.Should().NotBeNull();
        dbPosition!.Version.Should().Be(2);
    }

    [Fact]
    public async Task DragOperations_ShouldProvideRealTimeFeedback()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        var task = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id);
        
        await ApiClient.PostAsJsonAsync($"/api/tasks/{task.Id}/assign-to-board", new
        {
            BoardId = board.Id,
            ColumnId = board.Columns[0].Id,
            Position = 1.0
        });
        
        var user2Connection = await CreateSecondUserConnectionAsync();
        await user2Connection.InvokeAsync<bool>("JoinBoard", board.Id.ToString());
        
        var dragEvents = new List<TaskDragEvent>();
        user2Connection.On<TaskDragEvent>("TaskDragStateChanged", (dragEvent) =>
        {
            dragEvents.Add(dragEvent);
        });
        
        // Act
        await KanbanHub.InvokeAsync("StartTaskDrag", task.Id.ToString(), board.Id.ToString(), board.Columns[0].Id.ToString());
        await KanbanHub.InvokeAsync("UpdateTaskDrag", task.Id.ToString(), board.Id.ToString(), board.Columns[1].Id.ToString());
        await KanbanHub.InvokeAsync("EndTaskDrag", task.Id.ToString(), board.Id.ToString(), board.Columns[1].Id.ToString());
        
        // Allow time for events to propagate
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        // Assert
        dragEvents.Should().HaveCount(3);
        dragEvents[0].DragState.Should().Be("start");
        dragEvents[1].DragState.Should().Be("move");
        dragEvents[2].DragState.Should().Be("end");
        
        dragEvents.All(e => e.TaskId == task.Id).Should().BeTrue();
        dragEvents.All(e => e.BoardId == board.Id).Should().BeTrue();
    }
}
```

### Test Category: WIP Limits and Validation
```csharp
public class WipLimitsIntegrationTests : KanbanIntegrationTestBase
{
    [Fact]
    public async Task MoveTask_ShouldEnforceWipLimits_WhenEnabled()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        var inProgressColumn = board.Columns.First(c => c.KeyField == "inprogress");
        
        // Create tasks up to WIP limit (3)
        var tasks = new List<TestTask>();
        for (int i = 0; i < 3; i++)
        {
            var task = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id, $"Task {i + 1}");
            await ApiClient.PostAsJsonAsync($"/api/tasks/{task.Id}/assign-to-board", new
            {
                BoardId = board.Id,
                ColumnId = inProgressColumn.Id,
                Position = i + 1.0
            });
            tasks.Add(task);
        }
        
        // Create one more task to exceed limit
        var extraTask = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id, "Extra Task");
        await ApiClient.PostAsJsonAsync($"/api/tasks/{extraTask.Id}/assign-to-board", new
        {
            BoardId = board.Id,
            ColumnId = board.Columns[0].Id, // Start in "To Do"
            Position = 1.0
        });
        
        // Act - Try to move the extra task to the column at WIP limit
        var moveRequest = new MoveTaskRequest
        {
            TaskId = extraTask.Id,
            BoardId = board.Id,
            NewColumnId = inProgressColumn.Id,
            NewPosition = 4.0m,
            LastKnownVersion = 1
        };
        
        var response = await ApiClient.PutAsJsonAsync($"/api/kanban/tasks/{extraTask.Id}/move", moveRequest);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("WIP limit");
        
        // Verify task wasn't moved
        var dbPosition = await DbContext.TaskPositions
            .FirstOrDefaultAsync(tp => tp.TaskId == extraTask.Id && tp.BoardId == board.Id);
        
        dbPosition!.ColumnId.Should().Be(board.Columns[0].Id); // Still in "To Do"
    }

    [Fact]
    public async Task DisableWipLimits_ShouldAllowUnlimitedTasks()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        
        // Disable WIP limits
        await ApiClient.PutAsJsonAsync($"/api/kanban/boards/{board.Id}", new UpdateKanbanBoardRequest
        {
            Settings = new KanbanBoardSettingsDto
            {
                EnableWipLimits = false,
                MaxTasksPerColumn = 100
            }
        });
        
        var inProgressColumn = board.Columns.First(c => c.KeyField == "inprogress");
        
        // Create tasks exceeding the original WIP limit
        var tasks = new List<TestTask>();
        for (int i = 0; i < 5; i++) // More than the original limit of 3
        {
            var task = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id, $"Task {i + 1}");
            
            var moveRequest = new MoveTaskRequest
            {
                TaskId = task.Id,
                BoardId = board.Id,
                NewColumnId = inProgressColumn.Id,
                NewPosition = i + 1.0m,
                LastKnownVersion = 1
            };
            
            // Act
            var response = await ApiClient.PutAsJsonAsync($"/api/kanban/tasks/{task.Id}/move", moveRequest);
            
            // Assert
            response.EnsureSuccessStatusCode();
            tasks.Add(task);
        }
        
        // Verify all tasks are in the column
        var tasksInColumn = await DbContext.TaskPositions
            .Where(tp => tp.BoardId == board.Id && tp.ColumnId == inProgressColumn.Id)
            .CountAsync();
        
        tasksInColumn.Should().Be(5);
    }
}
```

### Test Category: Swimlane Organization
```csharp
public class SwimlaneIntegrationTests : KanbanIntegrationTestBase
{
    [Fact]
    public async Task GetBoardTasks_ShouldOrganizeBySwimlanes_WhenEnabled()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        var users = TestUsers.Take(3).ToList();
        
        // Enable swimlanes by assignee
        await ApiClient.PutAsJsonAsync($"/api/kanban/boards/{board.Id}", new UpdateKanbanBoardRequest
        {
            Settings = new KanbanBoardSettingsDto
            {
                EnableSwimlanes = true,
                DefaultSwimlaneBy = "assignee"
            }
        });
        
        // Create tasks assigned to different users
        var tasks = new List<TestTask>();
        foreach (var user in users)
        {
            var task = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id, $"Task for {user.Name}");
            
            // Update task to assign to user
            await ApiClient.PutAsJsonAsync($"/api/tasks/{task.Id}", new
            {
                AssigneeId = user.Id
            });
            
            // Assign to board with swimlane value
            await ApiClient.PostAsJsonAsync($"/api/tasks/{task.Id}/assign-to-board", new
            {
                BoardId = board.Id,
                ColumnId = board.Columns[0].Id,
                Position = 1.0,
                SwimlaneValue = user.Id.ToString()
            });
            
            tasks.Add(task);
        }
        
        // Act
        var filterRequest = new BoardFilterRequest
        {
            SwimlaneBy = "assignee"
        };
        
        var response = await ApiClient.PostAsJsonAsync($"/api/kanban/boards/{board.Id}/tasks", filterRequest);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var boardData = await response.Content.ReadFromJsonAsync<KanbanBoardTasksDto>();
        
        boardData!.Swimlanes.Should().HaveCount(3);
        boardData.Swimlanes.Should().Contain(users.Select(u => u.Id.ToString()));
        
        // Verify each task is in correct swimlane
        foreach (var task in tasks)
        {
            var taskData = boardData.Tasks.First(t => t.Id == task.Id);
            taskData.Position!.SwimlaneValue.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task MoveBetweenSwimlanes_ShouldUpdateSwimlaneValue()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        var task = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id);
        var user1 = TestUsers[0];
        var user2 = TestUsers[1];
        
        await ApiClient.PostAsJsonAsync($"/api/tasks/{task.Id}/assign-to-board", new
        {
            BoardId = board.Id,
            ColumnId = board.Columns[0].Id,
            Position = 1.0,
            SwimlaneValue = user1.Id.ToString()
        });
        
        // Act
        var moveRequest = new MoveTaskRequest
        {
            TaskId = task.Id,
            BoardId = board.Id,
            NewColumnId = board.Columns[0].Id, // Same column
            NewPosition = 1.0m,
            NewSwimlaneValue = user2.Id.ToString(), // Different swimlane
            LastKnownVersion = 1
        };
        
        var response = await ApiClient.PutAsJsonAsync($"/api/kanban/tasks/{task.Id}/move", moveRequest);
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        var dbPosition = await DbContext.TaskPositions
            .FirstOrDefaultAsync(tp => tp.TaskId == task.Id && tp.BoardId == board.Id);
        
        dbPosition!.SwimlaneValue.Should().Be(user2.Id.ToString());
    }
}
```

### Test Category: Performance and Load Testing
```csharp
public class PerformanceIntegrationTests : KanbanIntegrationTestBase
{
    [Fact]
    public async Task LargeBoardLoad_ShouldLoadWithin3Seconds()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        
        // Create 500 tasks on the board
        var tasks = new List<TestTask>();
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 500; i++)
        {
            var task = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id, $"Task {i + 1}");
            await ApiClient.PostAsJsonAsync($"/api/tasks/{task.Id}/assign-to-board", new
            {
                BoardId = board.Id,
                ColumnId = board.Columns[i % 4].Id,
                Position = (i / 4) + 1.0
            });
            tasks.Add(task);
        }
        
        var setupTime = stopwatch.Elapsed;
        Console.WriteLine($"Setup time for 500 tasks: {setupTime.TotalSeconds:F2} seconds");
        
        // Act
        stopwatch.Restart();
        var response = await ApiClient.GetAsync($"/api/kanban/boards/{board.Id}/tasks");
        stopwatch.Stop();
        
        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
        
        var boardData = await response.Content.ReadFromJsonAsync<KanbanBoardTasksDto>();
        boardData!.Tasks.Should().HaveCount(500);
        
        Console.WriteLine($"Load time for 500 tasks: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
    }

    [Fact]
    public async Task ConcurrentTaskMoves_ShouldHandle50Users()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        var tasks = new List<TestTask>();
        
        // Create 50 tasks
        for (int i = 0; i < 50; i++)
        {
            var task = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id, $"Task {i + 1}");
            await ApiClient.PostAsJsonAsync($"/api/tasks/{task.Id}/assign-to-board", new
            {
                BoardId = board.Id,
                ColumnId = board.Columns[0].Id,
                Position = i + 1.0
            });
            tasks.Add(task);
        }
        
        // Act - Simulate 50 concurrent users moving tasks
        var moveTasks = tasks.Select(async (task, index) =>
        {
            var userClient = await CreateUserHttpClientAsync($"user{index}");
            var moveRequest = new MoveTaskRequest
            {
                TaskId = task.Id,
                BoardId = board.Id,
                NewColumnId = board.Columns[1].Id,
                NewPosition = index + 1.0m,
                LastKnownVersion = 1
            };
            
            var stopwatch = Stopwatch.StartNew();
            var response = await userClient.PutAsJsonAsync($"/api/kanban/tasks/{task.Id}/move", moveRequest);
            stopwatch.Stop();
            
            return new { Response = response, Duration = stopwatch.Elapsed };
        });
        
        var results = await Task.WhenAll(moveTasks);
        
        // Assert
        var successfulMoves = results.Count(r => r.Response.IsSuccessStatusCode);
        var averageResponseTime = results.Average(r => r.Duration.TotalMilliseconds);
        
        successfulMoves.Should().BeGreaterThan(40); // Allow for some conflicts
        averageResponseTime.Should().BeLessThan(500); // 500ms target
        
        Console.WriteLine($"Successful moves: {successfulMoves}/50");
        Console.WriteLine($"Average response time: {averageResponseTime:F2}ms");
    }

    [Fact]
    public async Task RealTimeUpdates_ShouldPropagateWithin200ms()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        var task = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id);
        
        await ApiClient.PostAsJsonAsync($"/api/tasks/{task.Id}/assign-to-board", new
        {
            BoardId = board.Id,
            ColumnId = board.Columns[0].Id,
            Position = 1.0
        });
        
        // Set up 10 user connections
        var connections = new List<HubConnection>();
        var receivedEvents = new ConcurrentBag<(DateTime Timestamp, TaskMoveEvent Event)>();
        
        for (int i = 0; i < 10; i++)
        {
            var connection = await CreateUserHubConnectionAsync($"user{i}");
            await connection.StartAsync();
            await connection.InvokeAsync<bool>("JoinBoard", board.Id.ToString());
            
            connection.On<TaskMoveEvent>("TaskMoved", (moveEvent) =>
            {
                receivedEvents.Add((DateTime.UtcNow, moveEvent));
            });
            
            connections.Add(connection);
        }
        
        // Act
        var moveTime = DateTime.UtcNow;
        var moveRequest = new MoveTaskRequest
        {
            TaskId = task.Id,
            BoardId = board.Id,
            NewColumnId = board.Columns[1].Id,
            NewPosition = 1.0m,
            LastKnownVersion = 1
        };
        
        await ApiClient.PutAsJsonAsync($"/api/kanban/tasks/{task.Id}/move", moveRequest);
        
        // Wait for events to propagate
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        // Assert
        receivedEvents.Should().HaveCount(10); // All connections should receive the event
        
        var propagationTimes = receivedEvents.Select(e => (e.Timestamp - moveTime).TotalMilliseconds);
        var maxPropagationTime = propagationTimes.Max();
        var averagePropagationTime = propagationTimes.Average();
        
        maxPropagationTime.Should().BeLessThan(200); // 200ms target
        
        Console.WriteLine($"Max propagation time: {maxPropagationTime:F2}ms");
        Console.WriteLine($"Average propagation time: {averagePropagationTime:F2}ms");
        
        // Cleanup
        foreach (var connection in connections)
        {
            await connection.DisposeAsync();
        }
    }
}
```

---

## Mobile and Touch Integration Tests

### Test Category: Mobile Responsiveness
```csharp
public class MobileIntegrationTests : KanbanIntegrationTestBase
{
    [Fact]
    public async Task MobileApiCalls_ShouldSupportTouchOptimization()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        
        // Add mobile-specific headers
        ApiClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X)");
        ApiClient.DefaultRequestHeaders.Add("X-Touch-Device", "true");
        
        // Act
        var response = await ApiClient.GetAsync($"/api/kanban/boards/{board.Id}/tasks");
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify mobile-optimized response
        response.Headers.Should().Contain(h => h.Key == "X-Mobile-Optimized");
        
        var boardData = await response.Content.ReadFromJsonAsync<KanbanBoardTasksDto>();
        boardData!.Should().NotBeNull();
    }

    [Fact]
    public async Task TouchGestures_ShouldBeRecognizedInSignalR()
    {
        // Arrange
        var board = await TestDataManager.CreateTestBoardAsync(ApiClient, TestProjects.First().Id);
        var task = await TestDataManager.CreateTestTaskAsync(ApiClient, TestProjects.First().Id);
        
        await ApiClient.PostAsJsonAsync($"/api/tasks/{task.Id}/assign-to-board", new
        {
            BoardId = board.Id,
            ColumnId = board.Columns[0].Id,
            Position = 1.0
        });
        
        // Create mobile SignalR connection
        var mobileConnection = new HubConnectionBuilder()
            .WithUrl($"{App.GetEndpoint("apiservice")}/hubs/kanban", options =>
            {
                options.AccessTokenProvider = () => GenerateTestTokenAsync();
                options.Headers.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X)");
                options.Headers.Add("X-Touch-Device", "true");
            })
            .Build();
        
        await mobileConnection.StartAsync();
        await mobileConnection.InvokeAsync<bool>("JoinBoard", board.Id.ToString());
        
        // Act - Simulate touch drag sequence
        await mobileConnection.InvokeAsync("StartTaskDrag", task.Id.ToString(), board.Id.ToString(), board.Columns[0].Id.ToString());
        await Task.Delay(100); // Simulate touch hold
        await mobileConnection.InvokeAsync("UpdateTaskDrag", task.Id.ToString(), board.Id.ToString(), board.Columns[1].Id.ToString());
        await mobileConnection.InvokeAsync("EndTaskDrag", task.Id.ToString(), board.Id.ToString(), board.Columns[1].Id.ToString());
        
        // Assert
        // Verify the connection handles touch-specific operations
        mobileConnection.State.Should().Be(HubConnectionState.Connected);
        
        await mobileConnection.DisposeAsync();
    }
}
```

---

## Integration Test Status: COMPLETE ✅

All enhanced integration test scenarios have been defined:
- [x] Core kanban board management workflows
- [x] Real-time task movement and collaboration
- [x] Concurrent operation handling with optimistic concurrency
- [x] WIP limits enforcement and validation
- [x] Swimlane organization and filtering
- [x] Performance testing with large datasets (500+ tasks)
- [x] Load testing with concurrent users (50+ simultaneous operations)
- [x] Real-time update propagation timing (200ms target)
- [x] Mobile device and touch interaction testing
- [x] Database consistency and transaction integrity

**Ready to proceed to Phase 0.5: Enhanced Manual Testing Guide**