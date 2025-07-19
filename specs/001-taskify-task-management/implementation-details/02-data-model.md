# Data Model: EF Core Entities and Database Schema

**Created**: 2025-07-19  
**Status**: Ready for Implementation  

---

## Entity Design Strategy

**Approach**: Single model representation across API and UI (Constitutional Article VIII)
- Same entities used for database, API responses, and Blazor components
- No separate DTOs unless serialization requirements differ
- Direct EF Core entity usage in controllers and Blazor pages

## Core Entities

### 1. User Entity
```csharp
public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? ProfileImageUrl { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public virtual ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public virtual ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public virtual ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public virtual ICollection<Notification> ReceivedNotifications { get; set; } = new List<Notification>();
}
```

### 2. Project Entity
```csharp
public class Project
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public string OwnerId { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
```

### 3. ProjectMember Entity (Join Table)
```csharp
public class ProjectMember
{
    public string ProjectId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public ProjectRole Role { get; set; } = ProjectRole.Member;
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public enum ProjectRole
{
    Member = 1,
    Manager = 2,
    Owner = 3
}
```

### 4. TaskItem Entity
```csharp
public class TaskItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Required]
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    
    [Required]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
    public int Position { get; set; } = 0; // For kanban ordering
    
    [Required]
    public string ProjectId { get; set; } = string.Empty;
    
    [Required]
    public string CreatedById { get; set; } = string.Empty;
    
    public string? AssignedToId { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User CreatedBy { get; set; } = null!;
    public virtual User? AssignedTo { get; set; }
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

public enum TaskStatus
{
    ToDo = 1,
    InProgress = 2,
    Done = 3
}

public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
```

### 5. Notification Entity
```csharp
public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public NotificationType Type { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public string? TaskId { get; set; }
    
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual TaskItem? Task { get; set; }
}

public enum NotificationType
{
    TaskAssigned = 1,
    TaskStatusChanged = 2,
    TaskDueSoon = 3,
    ProjectInvitation = 4
}
```

## Database Context Configuration

### TaskifyDbContext
```csharp
public class TaskifyDbContext : DbContext
{
    public TaskifyDbContext(DbContextOptions<TaskifyDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigureUser(modelBuilder);
        ConfigureProject(modelBuilder);
        ConfigureProjectMember(modelBuilder);
        ConfigureTaskItem(modelBuilder);
        ConfigureNotification(modelBuilder);
        
        SeedInitialData(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureProject(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            
            entity.HasOne(e => e.Owner)
                  .WithMany(e => e.OwnedProjects)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureProjectMember(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(e => new { e.ProjectId, e.UserId });
            
            entity.HasOne(e => e.Project)
                  .WithMany(e => e.Members)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.User)
                  .WithMany(e => e.ProjectMemberships)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureTaskItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            
            entity.HasOne(e => e.Project)
                  .WithMany(e => e.Tasks)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.CreatedBy)
                  .WithMany(e => e.CreatedTasks)
                  .HasForeignKey(e => e.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.AssignedTo)
                  .WithMany(e => e.AssignedTasks)
                  .HasForeignKey(e => e.AssignedToId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            // Index for efficient kanban queries
            entity.HasIndex(e => new { e.ProjectId, e.Status, e.Position });
            
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureNotification(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            
            entity.HasOne(e => e.User)
                  .WithMany(e => e.ReceivedNotifications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Task)
                  .WithMany(e => e.Notifications)
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Index for efficient notification queries
            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt });
            
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Seed default admin user
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = "admin-user-id",
                Name = "Admin User",
                Email = "admin@taskify.local",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed sample project
        modelBuilder.Entity<Project>().HasData(
            new Project
            {
                Id = "sample-project-id",
                Name = "Sample Project",
                Description = "A sample project for testing",
                OwnerId = "admin-user-id",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed sample tasks
        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem
            {
                Id = "sample-task-1",
                Title = "Sample Task 1",
                Description = "This is a sample task in To Do status",
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Medium,
                Position = 1,
                ProjectId = "sample-project-id",
                CreatedById = "admin-user-id",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = "sample-task-2",
                Title = "Sample Task 2",
                Description = "This is a sample task in In Progress status",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                Position = 1,
                ProjectId = "sample-project-id",
                CreatedById = "admin-user-id",
                AssignedToId = "admin-user-id",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
```

## Repository Pattern (Optional)

Since we're following Constitutional simplicity, we'll use EF Core directly in controllers. However, if needed:

```csharp
public interface ITaskRepository
{
    Task<List<TaskItem>> GetProjectTasksAsync(string projectId);
    Task<TaskItem?> GetTaskByIdAsync(string taskId);
    Task<TaskItem> CreateTaskAsync(TaskItem task);
    Task<TaskItem> UpdateTaskAsync(TaskItem task);
    Task DeleteTaskAsync(string taskId);
}

public class TaskRepository : ITaskRepository
{
    private readonly TaskifyDbContext _context;

    public TaskRepository(TaskifyDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskItem>> GetProjectTasksAsync(string projectId)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .OrderBy(t => t.Status)
            .ThenBy(t => t.Position)
            .ToListAsync();
    }

    // Other methods...
}
```

## Migration Commands

```bash
# Add migration
dotnet ef migrations add InitialCreate -p Taskify.ApiService

# Update database
dotnet ef database update -p Taskify.ApiService

# Generate SQL script
dotnet ef migrations script -p Taskify.ApiService -o migration.sql
```

## Performance Considerations

### Indexes for Kanban Queries
- Composite index on `(ProjectId, Status, Position)` for efficient kanban loading
- Index on `(UserId, IsRead, CreatedAt)` for notification queries
- Unique index on `User.Email` for authentication

### Query Optimization
- Use `Include()` for related data to avoid N+1 queries
- Consider `AsNoTracking()` for read-only queries
- Use pagination for large task lists

### Connection Pooling
EF Core context pooling is configured in Program.cs:
```csharp
builder.AddNpgsqlDbContext<TaskifyDbContext>("taskdb", 
    configureSettings: settings => 
    {
        settings.DbContextPooling = true;
    });
```

## Next Steps

1. **Create migration**: Run EF Core migration commands
2. **API implementation**: See **03-api-contracts.md** for controller design
3. **Integration**: Reference this model in **05-integrations.md** for SignalR and SMTP

## Validation Rules

Entity validation is handled via Data Annotations, but additional business rules:

- **Task Assignment**: Only project members can be assigned tasks
- **Project Ownership**: Only owners can delete projects
- **Task Status**: Status transitions should follow business logic (ToDo → InProgress → Done)
- **Position Management**: Maintain unique positions within each status column