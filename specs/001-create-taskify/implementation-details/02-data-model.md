# Data Model: Entity Framework Entities

## Core Entities

### User Entity
```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Product Manager" or "Engineer"
    
    // Navigation properties
    public List<Task> AssignedTasks { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}
```

### Project Entity
```csharp
public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    
    // Navigation properties
    public List<Task> Tasks { get; set; } = new();
}
```

### Task Entity
```csharp
public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    
    // Foreign keys
    public int ProjectId { get; set; }
    public int? AssignedUserId { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public User? AssignedUser { get; set; }
    public List<Comment> Comments { get; set; } = new();
}
```

### Comment Entity
```csharp
public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    
    // Foreign keys
    public int TaskId { get; set; }
    public int AuthorId { get; set; }
    
    // Navigation properties
    public Task Task { get; set; } = null!;
    public User Author { get; set; } = null!;
}
```

## Enums

### TaskStatus
```csharp
public enum TaskStatus
{
    ToDo = 1,
    InProgress = 2,
    InReview = 3,
    Done = 4
}
```

## Entity Framework Configuration

### DbContext
```csharp
public class TaskifyDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Task> Tasks { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships and constraints
        ConfigureUserEntity(modelBuilder);
        ConfigureProjectEntity(modelBuilder);
        ConfigureTaskEntity(modelBuilder);
        ConfigureCommentEntity(modelBuilder);
    }
}
```

## Relationships

- **User ↔ Task**: One-to-many (User can have multiple assigned tasks)
- **Project ↔ Task**: One-to-many (Project contains multiple tasks)
- **Task ↔ Comment**: One-to-many (Task can have multiple comments)
- **User ↔ Comment**: One-to-many (User can author multiple comments)

## Indexes

- `IX_Tasks_ProjectId_Status` - For efficient Kanban board queries
- `IX_Tasks_AssignedUserId` - For user-specific task filtering
- `IX_Comments_TaskId` - For task comment retrieval
- `IX_Comments_AuthorId` - For comment permission checks

## Seed Data Structure

### Predefined Users
1. Sarah Chen (Product Manager)
2. Alex Rodriguez (Senior Engineer)
3. Jordan Kim (Engineer)
4. Taylor Swift (Engineer)
5. Morgan Davis (Engineer)

### Sample Projects
1. **Mobile App Redesign** (21 tasks) - High activity
2. **API Integration Platform** (20 tasks) - Moderate activity
3. **Team Onboarding System** (20 tasks) - Low activity (maintenance)

### Task Distribution
- Tasks distributed across all four status columns
- Varied assignment across users to show different workloads
- Realistic titles and descriptions per project context