# Inter-Library Tests: Cross-Service Integration

## Web-API Boundary Tests

### Project Service Integration
```csharp
[Test]
public async Task WebToAPI_GetProjects_ShouldReturnFormattedData()
{
    // Arrange
    var apiClient = CreateAPIClient();
    var webService = new ProjectService(apiClient);
    
    // Act
    var projects = await webService.GetProjectsAsync();
    
    // Assert
    projects.Should().HaveCount(3);
    projects.Should().AllSatisfy(p => 
    {
        p.Id.Should().BePositive();
        p.Title.Should().NotBeEmpty();
        p.TaskCounts.Should().NotBeNull();
        p.TaskCounts.ToDo.Should().BeGreaterOrEqualTo(0);
        p.TaskCounts.InProgress.Should().BeGreaterOrEqualTo(0);
        p.TaskCounts.InReview.Should().BeGreaterOrEqualTo(0);
        p.TaskCounts.Done.Should().BeGreaterOrEqualTo(0);
    });
}
```

### Task Service Integration
```csharp
[Test]
public async Task WebToAPI_UpdateTaskStatus_ShouldPropagateChanges()
{
    // Arrange
    var apiClient = CreateAPIClient();
    var taskService = new TaskService(apiClient);
    var notificationService = new NotificationService();
    
    var taskId = 1;
    var newStatus = TaskStatus.InProgress;
    
    // Act
    var result = await taskService.UpdateTaskStatusAsync(taskId, newStatus);
    
    // Assert
    result.Should().BeTrue();
    
    // Verify notification was sent
    notificationService.LastNotification.Should().NotBeNull();
    notificationService.LastNotification.Type.Should().Be("TaskStatusChanged");
    notificationService.LastNotification.TaskId.Should().Be(taskId);
}
```

### Comment Service Integration
```csharp
[Test]
public async Task WebToAPI_AddComment_ShouldValidatePermissions()
{
    // Arrange
    var apiClient = CreateAPIClient();
    var commentService = new CommentService(apiClient);
    
    var taskId = 1;
    var comment = new CommentRequest
    {
        Content = "Test comment",
        AuthorId = 1
    };
    
    // Act
    var result = await commentService.AddCommentAsync(taskId, comment);
    
    // Assert
    result.Should().NotBeNull();
    result.Id.Should().BePositive();
    result.Content.Should().Be("Test comment");
    result.Author.Id.Should().Be(1);
    result.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
}
```

## SignalR Hub Integration Tests

### Notification Hub to Web Client
```csharp
[Test]
public async Task NotificationHub_TaskUpdate_ShouldNotifyWebClients()
{
    // Arrange
    var hubContext = CreateHubContext();
    var webClient = CreateWebClient();
    
    var receivedNotification = false;
    webClient.On<TaskStatusChangedNotification>("TaskStatusChanged", notification =>
    {
        receivedNotification = true;
        notification.TaskId.Should().Be(1);
        notification.NewStatus.Should().Be("InProgress");
    });
    
    // Act
    await hubContext.Clients.All.SendAsync("TaskStatusChanged", new TaskStatusChangedNotification
    {
        TaskId = 1,
        NewStatus = "InProgress",
        ProjectId = 1,
        Timestamp = DateTime.UtcNow
    });
    
    // Assert
    await Task.Delay(1000); // Wait for notification
    receivedNotification.Should().BeTrue();
}
```

### Project Group Notifications
```csharp
[Test]
public async Task NotificationHub_ProjectSpecificUpdates_ShouldNotifyCorrectClients()
{
    // Arrange
    var hubContext = CreateHubContext();
    var project1Client = CreateWebClient();
    var project2Client = CreateWebClient();
    
    // Join different project groups
    await hubContext.Groups.AddToGroupAsync(project1Client.ConnectionId, "Project_1");
    await hubContext.Groups.AddToGroupAsync(project2Client.ConnectionId, "Project_2");
    
    var project1Notified = false;
    var project2Notified = false;
    
    project1Client.On<TaskStatusChangedNotification>("TaskStatusChanged", _ => project1Notified = true);
    project2Client.On<TaskStatusChangedNotification>("TaskStatusChanged", _ => project2Notified = true);
    
    // Act - Send notification to Project 1 only
    await hubContext.Clients.Group("Project_1").SendAsync("TaskStatusChanged", new TaskStatusChangedNotification
    {
        TaskId = 1,
        NewStatus = "InProgress",
        ProjectId = 1,
        Timestamp = DateTime.UtcNow
    });
    
    // Assert
    await Task.Delay(1000);
    project1Notified.Should().BeTrue();
    project2Notified.Should().BeFalse();
}
```

## Database Integration Boundaries

### API to Database Transactions
```csharp
[Test]
public async Task APIToDatabase_TaskUpdate_ShouldMaintainConsistency()
{
    // Arrange
    var dbContext = CreateTestDbContext();
    var apiController = new TasksController(dbContext);
    
    var taskId = 1;
    var originalTask = await dbContext.Tasks.FindAsync(taskId);
    var originalStatus = originalTask.Status;
    
    // Act
    var result = await apiController.UpdateTask(taskId, new TaskUpdateRequest
    {
        Status = TaskStatus.InProgress
    });
    
    // Assert
    result.Should().BeOfType<OkObjectResult>();
    
    // Verify database updated
    var updatedTask = await dbContext.Tasks.FindAsync(taskId);
    updatedTask.Status.Should().Be(TaskStatus.InProgress);
    updatedTask.Status.Should().NotBe(originalStatus);
    updatedTask.LastModified.Should().BeAfter(originalTask.LastModified);
}
```

### Comment Permissions Across Services
```csharp
[Test]
public async Task CommentPermissions_ShouldEnforceAcrossServices()
{
    // Arrange
    var dbContext = CreateTestDbContext();
    var apiController = new TasksController(dbContext);
    var webService = new CommentService(CreateAPIClient());
    
    // Create comment by user 1
    var comment = await apiController.AddComment(1, new CommentRequest
    {
        Content = "Original comment",
        AuthorId = 1
    });
    
    // Act - Try to update comment as user 2
    var updateResult = await webService.UpdateCommentAsync(1, comment.Id, new CommentUpdateRequest
    {
        Content = "Updated comment",
        AuthorId = 2
    });
    
    // Assert
    updateResult.Should().BeFalse();
    
    // Verify comment unchanged
    var unchangedComment = await dbContext.Comments.FindAsync(comment.Id);
    unchangedComment.Content.Should().Be("Original comment");
}
```

## Error Propagation Tests

### API Error to Web Client
```csharp
[Test]
public async Task APIError_ShouldPropagateToWebClient()
{
    // Arrange
    var failingApiClient = CreateFailingAPIClient();
    var taskService = new TaskService(failingApiClient);
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<TaskServiceException>(() =>
        taskService.UpdateTaskStatusAsync(999, TaskStatus.InProgress));
    
    exception.Message.Should().Contain("Task not found");
    exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

### Database Error to API Response
```csharp
[Test]
public async Task DatabaseError_ShouldReturnAPIError()
{
    // Arrange
    var failingDbContext = CreateFailingDbContext();
    var apiController = new TasksController(failingDbContext);
    
    // Act
    var result = await apiController.UpdateTask(1, new TaskUpdateRequest
    {
        Status = TaskStatus.InProgress
    });
    
    // Assert
    result.Should().BeOfType<StatusCodeResult>();
    var statusResult = result as StatusCodeResult;
    statusResult.StatusCode.Should().Be(500);
}
```

## Performance Boundary Tests

### End-to-End Performance
```csharp
[Test]
public async Task EndToEndTaskUpdate_ShouldCompleteWithinSLA()
{
    // Arrange
    var webApp = CreateFullWebApplication();
    var client = webApp.CreateClient();
    var hubConnection = await CreateHubConnection(webApp);
    
    var notificationReceived = false;
    hubConnection.On<TaskStatusChangedNotification>("TaskStatusChanged", _ => notificationReceived = true);
    
    var stopwatch = Stopwatch.StartNew();
    
    // Act - Full end-to-end update
    var response = await client.PutAsJsonAsync("/api/tasks/1", new { Status = "InProgress" });
    
    // Wait for notification
    var timeout = 0;
    while (!notificationReceived && timeout < 1000)
    {
        await Task.Delay(10);
        timeout += 10;
    }
    
    stopwatch.Stop();
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    notificationReceived.Should().BeTrue();
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // < 100ms SLA
}
```

### Concurrent Cross-Service Operations
```csharp
[Test]
public async Task ConcurrentCrossServiceOperations_ShouldMaintainConsistency()
{
    // Arrange
    var webApp = CreateFullWebApplication();
    var clients = Enumerable.Range(1, 5).Select(_ => webApp.CreateClient()).ToArray();
    
    // Act - Concurrent operations across services
    var tasks = clients.Select(async (client, index) =>
    {
        var taskId = index + 1;
        var updateResponse = await client.PutAsJsonAsync($"/api/tasks/{taskId}", new { Status = "InProgress" });
        var commentResponse = await client.PostAsJsonAsync($"/api/tasks/{taskId}/comments", new { Content = $"Comment {index}", AuthorId = 1 });
        return new { UpdateResponse = updateResponse, CommentResponse = commentResponse };
    });
    
    var results = await Task.WhenAll(tasks);
    
    // Assert
    results.Should().AllSatisfy(r =>
    {
        r.UpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        r.CommentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    });
}
```

## Test Infrastructure

### API Client Factory
```csharp
private HttpClient CreateAPIClient()
{
    var webApp = CreateAPIWebApplication();
    return webApp.CreateClient();
}

private HttpClient CreateFailingAPIClient()
{
    var handler = new Mock<HttpMessageHandler>();
    handler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));
    
    return new HttpClient(handler.Object);
}
```

### Full Application Factory
```csharp
private WebApplication CreateFullWebApplication()
{
    var builder = WebApplication.CreateBuilder();
    
    // Configure all services
    builder.Services.AddDbContext<TaskifyDbContext>(options =>
        options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
    
    builder.Services.AddSignalR();
    builder.Services.AddControllers();
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    
    // Add application services
    builder.Services.AddScoped<ITaskService, TaskService>();
    builder.Services.AddScoped<IProjectService, ProjectService>();
    builder.Services.AddScoped<ICommentService, CommentService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    
    var app = builder.Build();
    
    // Configure full pipeline
    app.UseRouting();
    app.MapControllers();
    app.MapRazorPages();
    app.MapBlazorHub();
    app.MapHub<NotificationHub>("/notifications");
    
    // Seed test data
    SeedTestData(app.Services);
    
    return app;
}
```

### Cross-Service Test Helpers
```csharp
private async Task<TaskResponse> CreateTestTask(int projectId, int assignedUserId)
{
    var apiClient = CreateAPIClient();
    var request = new TaskCreateRequest
    {
        Title = "Test Task",
        Description = "Test Description",
        ProjectId = projectId,
        AssignedUserId = assignedUserId
    };
    
    var response = await apiClient.PostAsJsonAsync("/api/tasks", request);
    return await response.Content.ReadFromJsonAsync<TaskResponse>();
}

private async Task<CommentResponse> CreateTestComment(int taskId, int authorId)
{
    var apiClient = CreateAPIClient();
    var request = new CommentRequest
    {
        Content = "Test Comment",
        AuthorId = authorId
    };
    
    var response = await apiClient.PostAsJsonAsync($"/api/tasks/{taskId}/comments", request);
    return await response.Content.ReadFromJsonAsync<CommentResponse>();
}
```