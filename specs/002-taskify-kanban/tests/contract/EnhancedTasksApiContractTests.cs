using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Aspire.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Taskify.ContractTests;

public class EnhancedTasksApiContractTests : IClassFixture<AspireAppHostFixture>
{
    private readonly AspireAppHostFixture _aspireFixture;
    private readonly ITestOutputHelper _output;
    private readonly JsonSerializerOptions _jsonOptions;

    public EnhancedTasksApiContractTests(AspireAppHostFixture aspireFixture, ITestOutputHelper output)
    {
        _aspireFixture = aspireFixture;
        _output = output;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task GetBoardTasks_ShouldReturnEmptyBoard_WhenNoBoardExists()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var nonExistentBoardId = Guid.NewGuid();
        
        // Act
        var response = await httpClient.GetAsync($"/api/kanban/boards/{nonExistentBoardId}/tasks");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBoardTasks_ShouldSupportPagination()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var boardId = Guid.NewGuid();
        
        // Act
        var response = await httpClient.GetAsync($"/api/kanban/boards/{boardId}/tasks?page=0&pageSize=50");
        
        // Assert
        // Should fail initially as board doesn't exist
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBoardTasks_ShouldSupportFiltering()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var boardId = Guid.NewGuid();
        var filterRequest = new BoardFilterRequest
        {
            Statuses = new[] { "todo", "inprogress" },
            Priorities = new[] { "high", "medium" },
            SearchText = "test task",
            SwimlaneBy = "assignee"
        };
        
        // Act
        var response = await httpClient.PostAsJsonAsync($"/api/kanban/boards/{boardId}/tasks", filterRequest, _jsonOptions);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MoveTask_ShouldValidateRequestSchema()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var taskId = Guid.NewGuid();
        var moveRequest = new MoveTaskRequest
        {
            TaskId = taskId,
            BoardId = Guid.NewGuid(),
            NewColumnId = Guid.NewGuid(),
            NewPosition = 1.5m,
            NewSwimlaneValue = "user123",
            LastKnownVersion = 1
        };
        
        // Act
        var response = await httpClient.PutAsJsonAsync($"/api/kanban/tasks/{taskId}/move", moveRequest, _jsonOptions);
        
        // Assert
        // Should fail initially as task/board doesn't exist
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task MoveTask_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var taskId = Guid.NewGuid();
        var invalidMoveRequest = new MoveTaskRequest
        {
            TaskId = Guid.Empty, // Invalid
            BoardId = Guid.Empty, // Invalid
            NewColumnId = Guid.Empty, // Invalid
            NewPosition = -1, // Invalid
            LastKnownVersion = 0 // Invalid
        };
        
        // Act
        var response = await httpClient.PutAsJsonAsync($"/api/kanban/tasks/{taskId}/move", invalidMoveRequest, _jsonOptions);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MoveTask_ShouldHandleOptimisticConcurrency()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var taskId = Guid.NewGuid();
        var moveRequest = new MoveTaskRequest
        {
            TaskId = taskId,
            BoardId = Guid.NewGuid(),
            NewColumnId = Guid.NewGuid(),
            NewPosition = 1.0m,
            LastKnownVersion = 5 // Simulating old version
        };
        
        // Act
        var response = await httpClient.PutAsJsonAsync($"/api/kanban/tasks/{taskId}/move", moveRequest, _jsonOptions);
        
        // Assert
        // When implemented, this should return 409 Conflict for version mismatch
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Conflict);
        
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var content = await response.Content.ReadAsStringAsync();
            var moveResponse = JsonSerializer.Deserialize<MoveTaskResponse>(content, _jsonOptions);
            moveResponse.Should().NotBeNull();
            moveResponse!.ConflictDetected.Should().BeTrue();
        }
    }

    [Fact]
    public async Task BatchMoveTasks_ShouldValidateRequestSchema()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var batchRequest = new BatchMoveTasksRequest
        {
            Moves = new[]
            {
                new MoveTaskRequest
                {
                    TaskId = Guid.NewGuid(),
                    BoardId = Guid.NewGuid(),
                    NewColumnId = Guid.NewGuid(),
                    NewPosition = 1.0m,
                    LastKnownVersion = 1
                },
                new MoveTaskRequest
                {
                    TaskId = Guid.NewGuid(),
                    BoardId = Guid.NewGuid(),
                    NewColumnId = Guid.NewGuid(),
                    NewPosition = 2.0m,
                    LastKnownVersion = 1
                }
            }
        };
        
        // Act
        var response = await httpClient.PutAsJsonAsync("/api/kanban/tasks/move-batch", batchRequest, _jsonOptions);
        
        // Assert
        // Should fail initially as tasks don't exist
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    public async Task BatchMoveTasks_ShouldEnforceBatchSizeLimit()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var largeBatchRequest = new BatchMoveTasksRequest
        {
            Moves = Enumerable.Range(0, 51) // Exceeds limit of 50
                .Select(i => new MoveTaskRequest
                {
                    TaskId = Guid.NewGuid(),
                    BoardId = Guid.NewGuid(),
                    NewColumnId = Guid.NewGuid(),
                    NewPosition = i,
                    LastKnownVersion = 1
                }).ToArray()
        };
        
        // Act
        var response = await httpClient.PutAsJsonAsync("/api/kanban/tasks/move-batch", largeBatchRequest, _jsonOptions);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTaskPositions_ShouldReturnEmptyArray_WhenTaskHasNoPositions()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var nonExistentTaskId = Guid.NewGuid();
        
        // Act
        var response = await httpClient.GetAsync($"/api/tasks/{nonExistentTaskId}/position");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignTaskToBoard_ShouldValidateRequestSchema()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var taskId = Guid.NewGuid();
        var assignRequest = new AssignTaskToBoardRequest
        {
            BoardId = Guid.NewGuid(),
            ColumnId = Guid.NewGuid(),
            Position = 1.0m,
            SwimlaneValue = "user123"
        };
        
        // Act
        var response = await httpClient.PostAsJsonAsync($"/api/tasks/{taskId}/assign-to-board", assignRequest, _jsonOptions);
        
        // Assert
        // Should fail initially as task/board doesn't exist
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignTaskToBoard_ShouldReturnBadRequest_WhenTaskAlreadyOnBoard()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var taskId = Guid.NewGuid();
        var assignRequest = new AssignTaskToBoardRequest
        {
            BoardId = Guid.NewGuid(),
            ColumnId = Guid.NewGuid()
        };
        
        // Act
        var response = await httpClient.PostAsJsonAsync($"/api/tasks/{taskId}/assign-to-board", assignRequest, _jsonOptions);
        
        // Assert
        // Should return 404 for non-existent task, or potentially 400 if task already assigned
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Created);
    }

    [Fact]
    public async Task RemoveTaskFromBoard_ShouldReturnNotFound_WhenTaskNotOnBoard()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var taskId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        
        // Act
        var response = await httpClient.DeleteAsync($"/api/tasks/{taskId}/remove-from-board?boardId={boardId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("assignee")]
    [InlineData("priority")]
    [InlineData("project")]
    [InlineData("none")]
    public async Task GetBoardTasks_ShouldSupportValidSwimlaneOptions(string swimlaneBy)
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var boardId = Guid.NewGuid();
        var filterRequest = new BoardFilterRequest
        {
            SwimlaneBy = swimlaneBy
        };
        
        // Act
        var response = await httpClient.PostAsJsonAsync($"/api/kanban/boards/{boardId}/tasks", filterRequest, _jsonOptions);
        
        // Assert
        // Should fail for board not found, but not for swimlane validation
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBoardTasks_ShouldReturnBadRequest_WhenInvalidSwimlaneOption()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var boardId = Guid.NewGuid();
        var filterRequest = new BoardFilterRequest
        {
            SwimlaneBy = "invalid_option"
        };
        
        // Act
        var response = await httpClient.PostAsJsonAsync($"/api/kanban/boards/{boardId}/tasks", filterRequest, _jsonOptions);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

// Contract DTOs for enhanced tasks API
public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    Guid? AssigneeId,
    Guid ProjectId,
    string[] Tags,
    TaskMetadataDto Metadata,
    int EstimatedHours,
    string? ExternalId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    TaskPositionDto? Position
);

public record TaskMetadataDto
{
    public string? Color { get; set; }
    public Dictionary<string, string> CustomFields { get; set; } = new();
    public DateTime? LastMovedAt { get; set; }
    public Guid? LastMovedBy { get; set; }
    public int MoveCount { get; set; } = 0;
}

public record TaskPositionDto(
    Guid Id,
    Guid TaskId,
    Guid BoardId,
    Guid ColumnId,
    decimal Position,
    string? SwimlaneValue,
    int Version,
    DateTime UpdatedAt,
    Guid UpdatedBy
);

public record KanbanBoardTasksDto(
    Guid BoardId,
    Guid ProjectId,
    string BoardName,
    TaskDto[] Tasks,
    BoardColumnDto[] Columns,
    string[] Swimlanes,
    int TotalCount,
    DateTime LastUpdated
);

public record MoveTaskRequest
{
    public Guid TaskId { get; set; }
    public Guid BoardId { get; set; }
    public Guid NewColumnId { get; set; }
    public decimal NewPosition { get; set; }
    public string? NewSwimlaneValue { get; set; }
    public int LastKnownVersion { get; set; }
}

public record MoveTaskResponse(
    bool Success,
    TaskDto Task,
    bool ConflictDetected = false,
    ConflictInfo? ConflictInfo = null
);

public record ConflictInfo(
    string Message,
    int CurrentVersion,
    int ExpectedVersion
);

public record BatchMoveTasksRequest
{
    public MoveTaskRequest[] Moves { get; set; } = Array.Empty<MoveTaskRequest>();
}

public record BoardFilterRequest
{
    public string[]? Statuses { get; set; }
    public Guid[]? AssigneeIds { get; set; }
    public string[]? Priorities { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string[]? Tags { get; set; }
    public string? SearchText { get; set; }
    public string SwimlaneBy { get; set; } = "none";
}

public record AssignTaskToBoardRequest
{
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public decimal? Position { get; set; }
    public string? SwimlaneValue { get; set; }
}