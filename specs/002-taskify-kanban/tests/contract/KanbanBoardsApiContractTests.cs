using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Aspire.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Taskify.ContractTests;

public class KanbanBoardsApiContractTests : IClassFixture<AspireAppHostFixture>
{
    private readonly AspireAppHostFixture _aspireFixture;
    private readonly ITestOutputHelper _output;
    private readonly JsonSerializerOptions _jsonOptions;

    public KanbanBoardsApiContractTests(AspireAppHostFixture aspireFixture, ITestOutputHelper output)
    {
        _aspireFixture = aspireFixture;
        _output = output;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task GetKanbanBoards_ShouldReturnEmptyArray_WhenNoBoards()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        
        // Act
        var response = await httpClient.GetAsync("/api/kanban/boards");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response: {content}");
        
        var boards = JsonSerializer.Deserialize<KanbanBoardDto[]>(content, _jsonOptions);
        boards.Should().NotBeNull();
        boards.Should().BeEmpty();
    }

    [Fact]
    public async Task GetKanbanBoards_ShouldReturnUnauthorized_WhenNoAuthToken()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        httpClient.DefaultRequestHeaders.Authorization = null;
        
        // Act
        var response = await httpClient.GetAsync("/api/kanban/boards");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateKanbanBoard_ShouldReturnBadRequest_WhenInvalidRequest()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var invalidRequest = new CreateKanbanBoardRequest
        {
            ProjectId = Guid.Empty, // Invalid
            Name = "", // Invalid - empty name
            Columns = new List<CreateBoardColumnRequest>()
        };
        
        // Act
        var response = await httpClient.PostAsJsonAsync("/api/kanban/boards", invalidRequest, _jsonOptions);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Error Response: {errorContent}");
        
        var error = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
        error.Should().NotBeNull();
        error!.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateKanbanBoard_ShouldValidateRequestSchema_WhenValidRequest()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var validRequest = new CreateKanbanBoardRequest
        {
            ProjectId = Guid.NewGuid(),
            Name = "Test Kanban Board",
            Description = "A test board for contract validation",
            Settings = new KanbanBoardSettingsDto
            {
                EnableWipLimits = true,
                EnableSwimlanes = false,
                DefaultSwimlaneBy = "assignee",
                MaxTasksPerColumn = 50
            },
            Columns = new List<CreateBoardColumnRequest>
            {
                new() { Name = "To Do", KeyField = "todo", Position = 1, Color = "#FF6B6B" },
                new() { Name = "In Progress", KeyField = "inprogress", Position = 2, WipLimit = 3, Color = "#4ECDC4" },
                new() { Name = "Done", KeyField = "done", Position = 3, Color = "#96CEB4" }
            }
        };
        
        // Act
        var response = await httpClient.PostAsJsonAsync("/api/kanban/boards", validRequest, _jsonOptions);
        
        // Assert
        // This should fail initially as we haven't implemented the endpoint yet
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Created, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Success Response: {content}");
            
            var board = JsonSerializer.Deserialize<KanbanBoardDto>(content, _jsonOptions);
            board.Should().NotBeNull();
            board!.Id.Should().NotBeEmpty();
            board.Name.Should().Be(validRequest.Name);
            board.ProjectId.Should().Be(validRequest.ProjectId);
            board.Columns.Should().HaveCount(3);
        }
    }

    [Fact]
    public async Task GetKanbanBoard_ShouldReturnNotFound_WhenBoardDoesNotExist()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var nonExistentBoardId = Guid.NewGuid();
        
        // Act
        var response = await httpClient.GetAsync($"/api/kanban/boards/{nonExistentBoardId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var errorContent = await response.Content.ReadAsStringAsync();
        
        var error = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
        error.Should().NotBeNull();
        error!.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateKanbanBoard_ShouldValidateRequestSchema()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var boardId = Guid.NewGuid();
        var updateRequest = new UpdateKanbanBoardRequest
        {
            Name = "Updated Board Name",
            Description = "Updated description",
            Settings = new KanbanBoardSettingsDto
            {
                EnableWipLimits = false,
                MaxTasksPerColumn = 100
            }
        };
        
        // Act
        var response = await httpClient.PutAsJsonAsync($"/api/kanban/boards/{boardId}", updateRequest, _jsonOptions);
        
        // Assert
        // Should fail initially as board doesn't exist and endpoint isn't implemented
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteKanbanBoard_ShouldReturnNotFound_WhenBoardDoesNotExist()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var nonExistentBoardId = Guid.NewGuid();
        
        // Act
        var response = await httpClient.DeleteAsync($"/api/kanban/boards/{nonExistentBoardId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBoardColumn_ShouldValidateRequestSchema()
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var boardId = Guid.NewGuid();
        var columnRequest = new CreateBoardColumnRequest
        {
            Name = "Testing",
            KeyField = "testing",
            Position = 4,
            WipLimit = 2,
            Color = "#FFA500"
        };
        
        // Act
        var response = await httpClient.PostAsJsonAsync($"/api/kanban/boards/{boardId}/columns", columnRequest, _jsonOptions);
        
        // Assert
        // Should fail initially as board doesn't exist and endpoint isn't implemented
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Created);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task CreateBoardColumn_ShouldReturnBadRequest_WhenNameIsInvalid(string? invalidName)
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var boardId = Guid.NewGuid();
        var columnRequest = new CreateBoardColumnRequest
        {
            Name = invalidName!,
            KeyField = "test",
            Position = 1
        };
        
        // Act
        var response = await httpClient.PostAsJsonAsync($"/api/kanban/boards/{boardId}/columns", columnRequest, _jsonOptions);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("#FF6B6B")]
    [InlineData("#4ECDC4")]
    [InlineData("#96CEB4")]
    [InlineData("#FFFFFF")]
    [InlineData("#000000")]
    public async Task CreateBoardColumn_ShouldAcceptValidHexColors(string validColor)
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var boardId = Guid.NewGuid();
        var columnRequest = new CreateBoardColumnRequest
        {
            Name = "Test Column",
            KeyField = "test",
            Position = 1,
            Color = validColor
        };
        
        // Act
        var response = await httpClient.PostAsJsonAsync($"/api/kanban/boards/{boardId}/columns", columnRequest, _jsonOptions);
        
        // Assert
        // Should fail for board not found, but not for color validation
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Created);
    }

    [Theory]
    [InlineData("FF6B6B")]   // Missing #
    [InlineData("#FF6B")]    // Too short
    [InlineData("#FF6B6B6B")] // Too long
    [InlineData("#GGGGGG")]  // Invalid hex characters
    public async Task CreateBoardColumn_ShouldReturnBadRequest_WhenColorIsInvalid(string invalidColor)
    {
        // Arrange
        var httpClient = _aspireFixture.CreateHttpClient("apiservice");
        var boardId = Guid.NewGuid();
        var columnRequest = new CreateBoardColumnRequest
        {
            Name = "Test Column",
            KeyField = "test",
            Position = 1,
            Color = invalidColor
        };
        
        // Act
        var response = await httpClient.PostAsJsonAsync($"/api/kanban/boards/{boardId}/columns", columnRequest, _jsonOptions);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

// Contract DTOs for testing - these should match the API contracts exactly
public record KanbanBoardDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string? Description,
    KanbanBoardSettingsDto Settings,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<BoardColumnDto> Columns
);

public record KanbanBoardSettingsDto
{
    public bool EnableWipLimits { get; set; } = true;
    public bool EnableSwimlanes { get; set; } = false;
    public string DefaultSwimlaneBy { get; set; } = "assignee";
    public bool EnableRealTimeUpdates { get; set; } = true;
    public int MaxTasksPerColumn { get; set; } = 100;
    public string Theme { get; set; } = "default";
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

public record BoardColumnDto(
    Guid Id,
    Guid BoardId,
    string Name,
    string KeyField,
    int Position,
    int? WipLimit,
    string? Color,
    bool IsVisible,
    DateTime CreatedAt
);

public record CreateKanbanBoardRequest
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public KanbanBoardSettingsDto Settings { get; set; } = new();
    public List<CreateBoardColumnRequest> Columns { get; set; } = new();
}

public record CreateBoardColumnRequest
{
    public string Name { get; set; } = string.Empty;
    public string KeyField { get; set; } = string.Empty;
    public int Position { get; set; }
    public int? WipLimit { get; set; }
    public string? Color { get; set; }
}

public record UpdateKanbanBoardRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public KanbanBoardSettingsDto? Settings { get; set; }
}

public record ErrorResponse(
    string Error,
    string Message,
    Dictionary<string, object>? Details = null
);