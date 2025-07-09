# Contract Tests: API Validation Scenarios

## Projects API Tests

### GET /api/projects
```csharp
[Test]
public async Task GetProjects_ShouldReturnAllProjectsWithTaskCounts()
{
    // Arrange
    var expectedProjects = new[]
    {
        new { Id = 1, Title = "Mobile App Redesign", TaskCounts = new { ToDo = 6, InProgress = 4, InReview = 3, Done = 8 } },
        new { Id = 2, Title = "API Integration Platform", TaskCounts = new { ToDo = 4, InProgress = 2, InReview = 2, Done = 12 } },
        new { Id = 3, Title = "Team Onboarding System", TaskCounts = new { ToDo = 2, InProgress = 1, InReview = 1, Done = 16 } }
    };
    
    // Act
    var response = await client.GetAsync("/api/projects");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var projects = await response.Content.ReadFromJsonAsync<ProjectsResponse>();
    projects.Projects.Should().HaveCount(3);
    // Verify task counts match expected distribution
}
```

### GET /api/projects/{id}
```csharp
[Test]
public async Task GetProject_WithValidId_ShouldReturnProjectWithTasks()
{
    // Arrange
    var projectId = 1;
    
    // Act
    var response = await client.GetAsync($"/api/projects/{projectId}");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();
    project.Id.Should().Be(projectId);
    project.Tasks.Should().NotBeNull();
    project.Tasks.ToDo.Should().HaveCount(6);
    project.Tasks.InProgress.Should().HaveCount(4);
    project.Tasks.InReview.Should().HaveCount(3);
    project.Tasks.Done.Should().HaveCount(8);
}
```

```csharp
[Test]
public async Task GetProject_WithInvalidId_ShouldReturn404()
{
    // Act
    var response = await client.GetAsync("/api/projects/999");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
    error.Error.Should().Be("Not Found");
}
```

## Tasks API Tests

### GET /api/tasks/{id}
```csharp
[Test]
public async Task GetTask_WithValidId_ShouldReturnTaskWithComments()
{
    // Arrange
    var taskId = 1;
    
    // Act
    var response = await client.GetAsync($"/api/tasks/{taskId}");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
    task.Id.Should().Be(taskId);
    task.Title.Should().NotBeEmpty();
    task.AssignedUser.Should().NotBeNull();
    task.Comments.Should().NotBeNull();
}
```

### PUT /api/tasks/{id}
```csharp
[Test]
public async Task UpdateTask_WithValidStatus_ShouldUpdateTaskStatus()
{
    // Arrange
    var taskId = 1;
    var updateRequest = new { Status = "InProgress", AssignedUserId = 2 };
    
    // Act
    var response = await client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
    task.Status.Should().Be("InProgress");
    task.AssignedUser.Id.Should().Be(2);
    task.LastModified.Should().BeAfter(task.Created);
}
```

```csharp
[Test]
public async Task UpdateTask_WithInvalidStatus_ShouldReturn400()
{
    // Arrange
    var taskId = 1;
    var updateRequest = new { Status = "InvalidStatus" };
    
    // Act
    var response = await client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
    error.Message.Should().Contain("Invalid task status");
}
```

### POST /api/tasks/{id}/comments
```csharp
[Test]
public async Task AddComment_WithValidData_ShouldCreateComment()
{
    // Arrange
    var taskId = 1;
    var commentRequest = new { Content = "Test comment", AuthorId = 1 };
    
    // Act
    var response = await client.PostAsJsonAsync($"/api/tasks/{taskId}/comments", commentRequest);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();
    comment.Content.Should().Be("Test comment");
    comment.Author.Id.Should().Be(1);
    comment.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
}
```

```csharp
[Test]
public async Task AddComment_WithInvalidTaskId_ShouldReturn404()
{
    // Arrange
    var taskId = 999;
    var commentRequest = new { Content = "Test comment", AuthorId = 1 };
    
    // Act
    var response = await client.PostAsJsonAsync($"/api/tasks/{taskId}/comments", commentRequest);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

### PUT /api/tasks/{taskId}/comments/{commentId}
```csharp
[Test]
public async Task UpdateComment_ByAuthor_ShouldUpdateComment()
{
    // Arrange
    var taskId = 1;
    var commentId = 1;
    var updateRequest = new { Content = "Updated comment", AuthorId = 1 };
    
    // Act
    var response = await client.PutAsJsonAsync($"/api/tasks/{taskId}/comments/{commentId}", updateRequest);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();
    comment.Content.Should().Be("Updated comment");
    comment.LastModified.Should().NotBeNull();
}
```

```csharp
[Test]
public async Task UpdateComment_ByNonAuthor_ShouldReturn403()
{
    // Arrange
    var taskId = 1;
    var commentId = 1; // Comment by user 1
    var updateRequest = new { Content = "Updated comment", AuthorId = 2 }; // Different user
    
    // Act
    var response = await client.PutAsJsonAsync($"/api/tasks/{taskId}/comments/{commentId}", updateRequest);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
    error.Message.Should().Contain("Cannot modify comment");
}
```

### DELETE /api/tasks/{taskId}/comments/{commentId}
```csharp
[Test]
public async Task DeleteComment_ByAuthor_ShouldDeleteComment()
{
    // Arrange
    var taskId = 1;
    var commentId = 1;
    
    // Act
    var response = await client.DeleteAsync($"/api/tasks/{taskId}/comments/{commentId}?authorId=1");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    
    // Verify comment is deleted
    var getResponse = await client.GetAsync($"/api/tasks/{taskId}");
    var task = await getResponse.Content.ReadFromJsonAsync<TaskResponse>();
    task.Comments.Should().NotContain(c => c.Id == commentId);
}
```

## SignalR Hub Tests

### Task Status Change Notification
```csharp
[Test]
public async Task TaskStatusChanged_ShouldNotifyAllClients()
{
    // Arrange
    var connection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5000/notifications")
        .Build();
    
    var receivedNotification = false;
    connection.On<TaskStatusChangedNotification>("TaskStatusChanged", notification =>
    {
        receivedNotification = true;
        notification.TaskId.Should().Be(1);
        notification.NewStatus.Should().Be("InProgress");
    });
    
    await connection.StartAsync();
    
    // Act
    await client.PutAsJsonAsync("/api/tasks/1", new { Status = "InProgress" });
    
    // Assert
    await Task.Delay(1000); // Wait for notification
    receivedNotification.Should().BeTrue();
}
```

## Error Scenarios

### Network Interruption During Drag Operation
```csharp
[Test]
public async Task UpdateTask_WithNetworkError_ShouldReturnAppropriateError()
{
    // Arrange
    var taskId = 1;
    var updateRequest = new { Status = "InProgress" };
    
    // Simulate network interruption
    client.Timeout = TimeSpan.FromMilliseconds(1);
    
    // Act & Assert
    await Assert.ThrowsAsync<TaskCanceledException>(() => 
        client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest));
}
```

### Concurrent Task Updates
```csharp
[Test]
public async Task UpdateTask_ConcurrentUpdates_ShouldHandleConflicts()
{
    // Arrange
    var taskId = 1;
    var update1 = new { Status = "InProgress" };
    var update2 = new { Status = "InReview" };
    
    // Act
    var task1 = client.PutAsJsonAsync($"/api/tasks/{taskId}", update1);
    var task2 = client.PutAsJsonAsync($"/api/tasks/{taskId}", update2);
    
    var responses = await Task.WhenAll(task1, task2);
    
    // Assert
    responses.Should().Contain(r => r.StatusCode == HttpStatusCode.OK);
    // Last update should win
}
```

## Performance Tests

### Drag Operation Response Time
```csharp
[Test]
public async Task UpdateTask_ShouldCompleteWithin100ms()
{
    // Arrange
    var taskId = 1;
    var updateRequest = new { Status = "InProgress" };
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    var response = await client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);
    stopwatch.Stop();
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
}
```

### Project Loading Time
```csharp
[Test]
public async Task GetProject_ShouldCompleteWithin2Seconds()
{
    // Arrange
    var projectId = 1;
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    var response = await client.GetAsync($"/api/projects/{projectId}");
    stopwatch.Stop();
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
}
```