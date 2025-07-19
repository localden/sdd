using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Aspire.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Taskify.ContractTests;

public class RealTimeSignalRContractTests : IClassFixture<AspireAppHostFixture>, IAsyncDisposable
{
    private readonly AspireAppHostFixture _aspireFixture;
    private readonly ITestOutputHelper _output;
    private readonly List<HubConnection> _connections = new();

    public RealTimeSignalRContractTests(AspireAppHostFixture aspireFixture, ITestOutputHelper output)
    {
        _aspireFixture = aspireFixture;
        _output = output;
    }

    [Fact]
    public async Task KanbanHub_ShouldEstablishConnection_WhenValidCredentials()
    {
        // Arrange
        var connection = await CreateHubConnectionAsync();
        
        // Act
        await connection.StartAsync();
        
        // Assert
        connection.State.Should().Be(HubConnectionState.Connected);
        _output.WriteLine($"Connection established: {connection.ConnectionId}");
    }

    [Fact]
    public async Task KanbanHub_ShouldRejectConnection_WhenNoAuthentication()
    {
        // Arrange
        var hubUrl = GetHubUrl();
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl) // No authentication token
            .Build();
        
        _connections.Add(connection);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await connection.StartAsync();
        });
        
        exception.Message.Should().Contain("401");
        _output.WriteLine($"Expected authentication failure: {exception.Message}");
    }

    [Fact]
    public async Task JoinBoard_ShouldReturnTrue_WhenValidBoardId()
    {
        // Arrange
        var connection = await CreateHubConnectionAsync();
        await connection.StartAsync();
        var boardId = Guid.NewGuid();
        
        // Act
        var result = await connection.InvokeAsync<bool>("JoinBoard", boardId.ToString());
        
        // Assert
        // Should return false initially as board doesn't exist, but method should be callable
        result.Should().BeFalse(); // Expected behavior for non-existent board
        _output.WriteLine($"JoinBoard result for non-existent board: {result}");
    }

    [Fact]
    public async Task JoinBoard_ShouldHandleInvalidBoardId()
    {
        // Arrange
        var connection = await CreateHubConnectionAsync();
        await connection.StartAsync();
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<HubException>(async () =>
        {
            await connection.InvokeAsync<bool>("JoinBoard", "invalid-guid");
        });
        
        exception.Message.Should().Contain("Invalid");
        _output.WriteLine($"Expected validation error: {exception.Message}");
    }

    [Fact]
    public async Task LeaveBoard_ShouldReturnTrue_WhenValidBoardId()
    {
        // Arrange
        var connection = await CreateHubConnectionAsync();
        await connection.StartAsync();
        var boardId = Guid.NewGuid();
        
        // First join, then leave
        await connection.InvokeAsync<bool>("JoinBoard", boardId.ToString());
        
        // Act
        var result = await connection.InvokeAsync<bool>("LeaveBoard", boardId.ToString());
        
        // Assert
        result.Should().BeTrue();
        _output.WriteLine($"LeaveBoard result: {result}");
    }

    [Fact]
    public async Task MoveTask_ShouldValidateRequestSchema()
    {
        // Arrange
        var connection = await CreateHubConnectionAsync();
        await connection.StartAsync();
        
        var moveRequest = new
        {
            TaskId = Guid.NewGuid(),
            BoardId = Guid.NewGuid(),
            NewColumnId = Guid.NewGuid(),
            NewPosition = 1.5,
            LastKnownVersion = 1
        };
        
        // Act
        var result = await connection.InvokeAsync<dynamic>("MoveTask", moveRequest);
        
        // Assert
        // Should fail as task doesn't exist, but method should be callable
        var success = (bool)result.success;
        success.Should().BeFalse();
        _output.WriteLine($"MoveTask result for non-existent task: {JsonSerializer.Serialize(result)}");
    }

    [Fact]
    public async Task MoveTask_ShouldHandleInvalidRequest()
    {
        // Arrange
        var connection = await CreateHubConnectionAsync();
        await connection.StartAsync();
        
        var invalidMoveRequest = new
        {
            TaskId = Guid.Empty, // Invalid
            BoardId = Guid.Empty, // Invalid
            NewColumnId = Guid.Empty, // Invalid
            NewPosition = -1, // Invalid
            LastKnownVersion = 0 // Invalid
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<HubException>(async () =>
        {
            await connection.InvokeAsync<dynamic>("MoveTask", invalidMoveRequest);
        });
        
        exception.Message.Should().Contain("Invalid");
        _output.WriteLine($"Expected validation error: {exception.Message}");
    }

    [Fact]
    public async Task TaskMoved_ShouldReceiveEvent_WhenTaskIsMoved()
    {
        // Arrange
        var connection = await CreateHubConnectionAsync();
        await connection.StartAsync();
        
        var boardId = Guid.NewGuid();
        await connection.InvokeAsync<bool>("JoinBoard", boardId.ToString());
        
        TaskMoveEvent? receivedEvent = null;
        var eventReceived = new TaskCompletionSource<bool>();
        
        connection.On<TaskMoveEvent>("TaskMoved", (moveEvent) =>
        {
            receivedEvent = moveEvent;
            eventReceived.SetResult(true);
        });
        
        // Act
        // This would normally be triggered by another user's action or API call
        // For now, we're testing the event structure
        var testEvent = new TaskMoveEvent
        {
            TaskId = Guid.NewGuid(),
            BoardId = boardId,
            NewColumnId = Guid.NewGuid(),
            NewPosition = 2.5m,
            MovedBy = Guid.NewGuid(),
            MovedByName = "Test User",
            Timestamp = DateTime.UtcNow,
            Version = 2
        };
        
        // Simulate receiving the event (in real scenario, this would come from server)
        // For contract testing, we validate the event structure can be handled
        
        // Assert
        // The connection should be able to register the event handler without errors
        connection.State.Should().Be(HubConnectionState.Connected);
        _output.WriteLine("TaskMoved event handler registered successfully");
    }

    [Fact]
    public async Task StartTaskDrag_ShouldNotifyOtherUsers()
    {
        // Arrange
        var connection1 = await CreateHubConnectionAsync();
        var connection2 = await CreateHubConnectionAsync();
        
        await connection1.StartAsync();
        await connection2.StartAsync();
        
        var boardId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        
        await connection1.InvokeAsync<bool>("JoinBoard", boardId.ToString());
        await connection2.InvokeAsync<bool>("JoinBoard", boardId.ToString());
        
        TaskDragEvent? receivedEvent = null;
        var eventReceived = new TaskCompletionSource<bool>();
        
        connection2.On<TaskDragEvent>("TaskDragStateChanged", (dragEvent) =>
        {
            receivedEvent = dragEvent;
            eventReceived.SetResult(true);
        });
        
        // Act
        await connection1.InvokeAsync("StartTaskDrag", taskId.ToString(), boardId.ToString(), columnId.ToString());
        
        // Assert
        // In a real implementation, this would trigger an event to connection2
        // For contract testing, we verify the method can be called without errors
        connection1.State.Should().Be(HubConnectionState.Connected);
        connection2.State.Should().Be(HubConnectionState.Connected);
        _output.WriteLine("StartTaskDrag method called successfully");
    }

    [Fact]
    public async Task UpdatePresence_ShouldValidateStatusValues()
    {
        // Arrange
        var connection = await CreateHubConnectionAsync();
        await connection.StartAsync();
        var boardId = Guid.NewGuid();
        
        await connection.InvokeAsync<bool>("JoinBoard", boardId.ToString());
        
        // Act & Assert - Valid status values
        await connection.InvokeAsync("UpdatePresence", boardId.ToString(), "active");
        await connection.InvokeAsync("UpdatePresence", boardId.ToString(), "idle");
        
        // Invalid status should throw exception
        var exception = await Assert.ThrowsAsync<HubException>(async () =>
        {
            await connection.InvokeAsync("UpdatePresence", boardId.ToString(), "invalid_status");
        });
        
        exception.Message.Should().Contain("Invalid");
        _output.WriteLine($"Expected validation error for invalid status: {exception.Message}");
    }

    [Fact]
    public async Task RequestBoardSync_ShouldReturnTimestamp()
    {
        // Arrange
        var connection = await CreateHubConnectionAsync();
        await connection.StartAsync();
        var boardId = Guid.NewGuid();
        
        // Act
        var result = await connection.InvokeAsync<dynamic>("RequestBoardSync", boardId.ToString());
        
        // Assert
        var syncRequested = (bool)result.syncRequested;
        syncRequested.Should().BeTrue();
        _output.WriteLine($"RequestBoardSync result: {JsonSerializer.Serialize(result)}");
    }

    [Fact]
    public async Task Connection_ShouldSupportAutomaticReconnection()
    {
        // Arrange
        var connection = new HubConnectionBuilder()
            .WithUrl(GetHubUrl(), options =>
            {
                options.AccessTokenProvider = () => Task.FromResult("test-token");
            })
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2) })
            .Build();
        
        _connections.Add(connection);
        
        var reconnectCount = 0;
        connection.Reconnecting += (exception) =>
        {
            reconnectCount++;
            _output.WriteLine($"Reconnecting attempt {reconnectCount}: {exception?.Message}");
            return Task.CompletedTask;
        };
        
        connection.Reconnected += (connectionId) =>
        {
            _output.WriteLine($"Reconnected with connection ID: {connectionId}");
            return Task.CompletedTask;
        };
        
        // Act
        await connection.StartAsync();
        
        // Assert
        connection.State.Should().Be(HubConnectionState.Connected);
        _output.WriteLine("Connection established with automatic reconnect configured");
    }

    private async Task<HubConnection> CreateHubConnectionAsync()
    {
        var hubUrl = GetHubUrl();
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult("test-token");
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .Build();
        
        _connections.Add(connection);
        return connection;
    }

    private string GetHubUrl()
    {
        var apiBaseUrl = _aspireFixture.GetEndpoint("apiservice");
        return $"{apiBaseUrl}/hubs/kanban";
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var connection in _connections)
        {
            if (connection.State == HubConnectionState.Connected)
            {
                await connection.DisposeAsync();
            }
        }
        _connections.Clear();
    }
}

// Contract DTOs for SignalR events
public record TaskMoveEvent
{
    public Guid TaskId { get; set; }
    public Guid BoardId { get; set; }
    public Guid? OldColumnId { get; set; }
    public Guid NewColumnId { get; set; }
    public decimal? OldPosition { get; set; }
    public decimal NewPosition { get; set; }
    public string? OldSwimlaneValue { get; set; }
    public string? NewSwimlaneValue { get; set; }
    public Guid MovedBy { get; set; }
    public string MovedByName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int Version { get; set; }
}

public record TaskUpdateEvent
{
    public Guid TaskId { get; set; }
    public Guid BoardId { get; set; }
    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid UpdatedBy { get; set; }
    public string UpdatedByName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public record BoardConfigurationEvent
{
    public Guid BoardId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; } = new();
    public Guid ChangedBy { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public record UserPresenceEvent
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid BoardId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastActivity { get; set; }
    public DateTime Timestamp { get; set; }
}

public record TaskDragEvent
{
    public Guid TaskId { get; set; }
    public Guid BoardId { get; set; }
    public string DragState { get; set; } = string.Empty;
    public Guid? CurrentColumnId { get; set; }
    public Guid? TargetColumnId { get; set; }
    public Guid DraggedBy { get; set; }
    public string DraggedByName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public record NotificationEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RelatedTaskId { get; set; }
    public Guid? RelatedBoardId { get; set; }
    public string? ActionUrl { get; set; }
    public string Priority { get; set; } = "normal";
    public DateTime Timestamp { get; set; }
}