# Integration Tests: Service Boundary Testing

## Web-API Integration Tests

### Project Dashboard Integration
```csharp
[Test]
public async Task ProjectDashboard_ShouldLoadProjectsFromAPI()
{
    // Arrange
    var webApp = await CreateWebApplication();
    var client = webApp.CreateClient();
    
    // Act
    var response = await client.GetAsync("/");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("Mobile App Redesign");
    content.Should().Contain("API Integration Platform");
    content.Should().Contain("Team Onboarding System");
}
```

### Kanban Board Integration
```csharp
[Test]
public async Task KanbanBoard_ShouldDisplayTasksFromAPI()
{
    // Arrange
    var webApp = await CreateWebApplication();
    var client = webApp.CreateClient();
    
    // Act
    var response = await client.GetAsync("/project/1");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("To Do");
    content.Should().Contain("In Progress");
    content.Should().Contain("In Review");
    content.Should().Contain("Done");
    content.Should().Contain("User research survey");
}
```

### Task Drag-and-Drop Integration
```csharp
[Test]
public async Task TaskDragDrop_ShouldUpdateTaskStatus()
{
    // Arrange
    var webApp = await CreateWebApplication();
    var client = webApp.CreateClient();
    
    // Act - Simulate drag-drop via API call
    var updateRequest = new { Status = "InProgress" };
    var response = await client.PutAsJsonAsync("/api/tasks/1", updateRequest);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    // Verify UI reflects change
    var pageResponse = await client.GetAsync("/project/1");
    var pageContent = await pageResponse.Content.ReadAsStringAsync();
    pageContent.Should().Contain("In Progress"); // Task moved to correct column
}
```

## Database Integration Tests

### Entity Framework Integration
```csharp
[Test]
public async Task TaskRepository_ShouldPersistTaskStatusChanges()
{
    // Arrange
    using var context = CreateTestDbContext();
    var taskRepository = new TaskRepository(context);
    var task = await taskRepository.GetByIdAsync(1);
    
    // Act
    task.Status = TaskStatus.InProgress;
    await taskRepository.UpdateAsync(task);
    
    // Assert
    var updatedTask = await taskRepository.GetByIdAsync(1);
    updatedTask.Status.Should().Be(TaskStatus.InProgress);
    updatedTask.LastModified.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
}
```

### User Assignment Integration
```csharp
[Test]
public async Task TaskAssignment_ShouldUpdateAssignedUser()
{
    // Arrange
    using var context = CreateTestDbContext();
    var taskRepository = new TaskRepository(context);
    var userRepository = new UserRepository(context);
    
    var task = await taskRepository.GetByIdAsync(1);
    var newAssignee = await userRepository.GetByIdAsync(2);
    
    // Act
    task.AssignedUserId = newAssignee.Id;
    await taskRepository.UpdateAsync(task);
    
    // Assert
    var updatedTask = await taskRepository.GetByIdAsync(1);
    updatedTask.AssignedUserId.Should().Be(2);
    updatedTask.AssignedUser.Name.Should().Be("Alex Rodriguez");
}
```

### Comment System Integration
```csharp
[Test]
public async Task CommentSystem_ShouldPersistCommentsWithAuthor()
{
    // Arrange
    using var context = CreateTestDbContext();
    var commentRepository = new CommentRepository(context);
    
    var comment = new Comment
    {
        Content = "Test comment",
        TaskId = 1,
        AuthorId = 1,
        Created = DateTime.UtcNow
    };
    
    // Act
    await commentRepository.AddAsync(comment);
    
    // Assert
    var savedComment = await commentRepository.GetByIdAsync(comment.Id);
    savedComment.Content.Should().Be("Test comment");
    savedComment.Author.Name.Should().Be("Sarah Chen");
    savedComment.Task.Title.Should().NotBeEmpty();
}
```

## SignalR Integration Tests

### Real-time Task Updates
```csharp
[Test]
public async Task TaskStatusUpdate_ShouldNotifyConnectedClients()
{
    // Arrange
    var webApp = await CreateWebApplication();
    var hubConnection = await CreateHubConnection(webApp);
    
    var receivedNotification = false;
    hubConnection.On<TaskStatusChangedNotification>("TaskStatusChanged", notification =>
    {
        receivedNotification = true;
        notification.TaskId.Should().Be(1);
        notification.NewStatus.Should().Be("InProgress");
    });
    
    // Act
    var client = webApp.CreateClient();
    await client.PutAsJsonAsync("/api/tasks/1", new { Status = "InProgress" });
    
    // Assert
    await Task.Delay(1000); // Wait for notification
    receivedNotification.Should().BeTrue();
}
```

### Comment Notifications
```csharp
[Test]
public async Task CommentAdded_ShouldNotifyProjectClients()
{
    // Arrange
    var webApp = await CreateWebApplication();
    var hubConnection = await CreateHubConnection(webApp);
    
    var receivedNotification = false;
    hubConnection.On<CommentAddedNotification>("CommentAdded", notification =>
    {
        receivedNotification = true;
        notification.Comment.Content.Should().Be("Test comment");
        notification.Comment.Author.Name.Should().Be("Sarah Chen");
    });
    
    // Act
    var client = webApp.CreateClient();
    await client.PostAsJsonAsync("/api/tasks/1/comments", new { Content = "Test comment", AuthorId = 1 });
    
    // Assert
    await Task.Delay(1000); // Wait for notification
    receivedNotification.Should().BeTrue();
}
```

## User Session Integration

### User Selection Persistence
```csharp
[Test]
public async Task UserSelection_ShouldPersistThroughoutSession()
{
    // Arrange
    var webApp = await CreateWebApplication();
    var client = webApp.CreateClient();
    
    // Act - Select user
    await client.PostAsJsonAsync("/api/session/user", new { UserId = 1 });
    
    // Assert - Verify user persisted
    var response = await client.GetAsync("/api/session/user");
    var user = await response.Content.ReadFromJsonAsync<UserResponse>();
    user.Id.Should().Be(1);
    user.Name.Should().Be("Sarah Chen");
}
```

### User Task Highlighting
```csharp
[Test]
public async Task UserTasks_ShouldBeHighlightedInUI()
{
    // Arrange
    var webApp = await CreateWebApplication();
    var client = webApp.CreateClient();
    
    // Act - Select user and load project
    await client.PostAsJsonAsync("/api/session/user", new { UserId = 1 });
    var response = await client.GetAsync("/project/1");
    
    // Assert - Verify user's tasks are highlighted
    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("user-assigned"); // CSS class for highlighting
}
```

## Error Handling Integration

### API Error Propagation
```csharp
[Test]
public async Task APIError_ShouldDisplayUserFriendlyMessage()
{
    // Arrange
    var webApp = await CreateWebApplication();
    var client = webApp.CreateClient();
    
    // Act - Attempt invalid operation
    var response = await client.PutAsJsonAsync("/api/tasks/999", new { Status = "InProgress" });
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    
    // Verify error is handled gracefully in UI
    var pageResponse = await client.GetAsync("/project/1");
    var pageContent = await pageResponse.Content.ReadAsStringAsync();
    pageContent.Should().Contain("error"); // Error message displayed
}
```

### Network Disconnection Handling
```csharp
[Test]
public async Task NetworkDisconnection_ShouldReconnectSignalR()
{
    // Arrange
    var webApp = await CreateWebApplication();
    var hubConnection = await CreateHubConnection(webApp);
    
    // Act - Simulate disconnection
    await hubConnection.StopAsync();
    
    // Assert - Verify reconnection
    await hubConnection.StartAsync();
    hubConnection.State.Should().Be(HubConnectionState.Connected);
}
```

## Performance Integration Tests

### Concurrent User Operations
```csharp
[Test]
public async Task ConcurrentUsers_ShouldHandleMultipleOperations()
{
    // Arrange
    var webApp = await CreateWebApplication();
    var clients = Enumerable.Range(1, 5).Select(_ => webApp.CreateClient()).ToArray();
    
    // Act - Concurrent task updates
    var tasks = clients.Select(async (client, index) =>
    {
        var updateRequest = new { Status = "InProgress" };
        return await client.PutAsJsonAsync($"/api/tasks/{index + 1}", updateRequest);
    });
    
    var responses = await Task.WhenAll(tasks);
    
    // Assert
    responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
}
```

### Database Connection Pooling
```csharp
[Test]
public async Task DatabaseConnections_ShouldHandleMultipleRequests()
{
    // Arrange
    var webApp = await CreateWebApplication();
    var client = webApp.CreateClient();
    
    // Act - Multiple simultaneous requests
    var tasks = Enumerable.Range(1, 10).Select(async i =>
    {
        return await client.GetAsync($"/api/projects/{i % 3 + 1}");
    });
    
    var responses = await Task.WhenAll(tasks);
    
    // Assert
    responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
}
```

## Test Infrastructure

### Test Database Setup
```csharp
private static TaskifyDbContext CreateTestDbContext()
{
    var options = new DbContextOptionsBuilder<TaskifyDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    
    var context = new TaskifyDbContext(options);
    SeedTestData(context);
    return context;
}

private static void SeedTestData(TaskifyDbContext context)
{
    // Add test users, projects, tasks, and comments
    var users = CreateTestUsers();
    var projects = CreateTestProjects();
    var tasks = CreateTestTasks();
    var comments = CreateTestComments();
    
    context.Users.AddRange(users);
    context.Projects.AddRange(projects);
    context.Tasks.AddRange(tasks);
    context.Comments.AddRange(comments);
    context.SaveChanges();
}
```

### Test Web Application Factory
```csharp
private async Task<WebApplication> CreateWebApplication()
{
    var builder = WebApplication.CreateBuilder();
    
    // Configure test services
    builder.Services.AddDbContext<TaskifyDbContext>(options =>
        options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
    
    builder.Services.AddSignalR();
    builder.Services.AddControllers();
    
    var app = builder.Build();
    
    // Configure test pipeline
    app.UseRouting();
    app.MapControllers();
    app.MapHub<NotificationHub>("/notifications");
    
    return app;
}
```

### SignalR Test Connection
```csharp
private async Task<HubConnection> CreateHubConnection(WebApplication app)
{
    var connection = new HubConnectionBuilder()
        .WithUrl($"http://localhost:5000/notifications", options =>
        {
            options.HttpMessageHandlerFactory = _ => app.CreateDefaultClient().CreateHandler();
        })
        .Build();
    
    await connection.StartAsync();
    return connection;
}
```