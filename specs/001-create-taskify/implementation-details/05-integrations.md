# Integrations: SignalR and PostgreSQL Integration Details

## SignalR Integration

### Hub Configuration
```csharp
// Startup.cs / Program.cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            // Configure connection timeout
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
            
            // Enable detailed errors in development
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
            
            // Configure message size limits
            options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
        });
        
        // Add CORS for SignalR
        services.AddCors(options =>
        {
            options.AddPolicy("SignalRCorsPolicy", builder =>
            {
                builder.WithOrigins("https://localhost:7000")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
            });
        });
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors("SignalRCorsPolicy");
        app.UseRouting();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<NotificationHub>("/notifications");
        });
    }
}
```

### Hub Implementation
```csharp
public class NotificationHub : Hub
{
    private readonly ITaskService _taskService;
    private readonly ILogger<NotificationHub> _logger;
    
    public NotificationHub(ITaskService taskService, ILogger<NotificationHub> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }
    
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation($"Client connected: {connectionId}");
        
        // Send initial connection success message
        await Clients.Caller.SendAsync("Connected", new { ConnectionId = connectionId });
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation($"Client disconnected: {connectionId}");
        
        // Remove from all groups
        await LeaveAllGroups(connectionId);
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task JoinProjectGroup(int projectId)
    {
        var groupName = $"Project_{projectId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation($"Client {Context.ConnectionId} joined {groupName}");
        
        // Send current project state
        await SendProjectState(projectId);
    }
    
    public async Task LeaveProjectGroup(int projectId)
    {
        var groupName = $"Project_{projectId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation($"Client {Context.ConnectionId} left {groupName}");
    }
    
    private async Task SendProjectState(int projectId)
    {
        try
        {
            var projectState = await _taskService.GetProjectStateAsync(projectId);
            await Clients.Caller.SendAsync("ProjectState", projectState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send project state for project {projectId}");
            await Clients.Caller.SendAsync("Error", "Failed to load project state");
        }
    }
    
    private async Task LeaveAllGroups(string connectionId)
    {
        // This would require tracking group memberships
        // For simplicity, we'll rely on SignalR's automatic cleanup
    }
}
```

### Client-Side SignalR Integration
```csharp
public class SignalRService : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<SignalRService> _logger;
    private readonly TaskCompletionSource<bool> _connectionTask;
    
    public SignalRService(ILogger<SignalRService> logger)
    {
        _logger = logger;
        _connectionTask = new TaskCompletionSource<bool>();
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7001/notifications")
            .WithAutomaticReconnect(new[] { 
                TimeSpan.Zero, 
                TimeSpan.FromSeconds(2), 
                TimeSpan.FromSeconds(10), 
                TimeSpan.FromSeconds(30) 
            })
            .Build();
        
        ConfigureEventHandlers();
    }
    
    private void ConfigureEventHandlers()
    {
        _hubConnection.On<TaskStatusChangedNotification>("TaskStatusChanged", OnTaskStatusChanged);
        _hubConnection.On<TaskAssignmentChangedNotification>("TaskAssignmentChanged", OnTaskAssignmentChanged);
        _hubConnection.On<CommentAddedNotification>("CommentAdded", OnCommentAdded);
        _hubConnection.On<ProjectTaskCountsNotification>("ProjectTaskCountsUpdated", OnProjectTaskCountsUpdated);
        
        _hubConnection.Closed += OnConnectionClosed;
        _hubConnection.Reconnected += OnReconnected;
        _hubConnection.Reconnecting += OnReconnecting;
    }
    
    public async Task StartAsync()
    {
        try
        {
            await _hubConnection.StartAsync();
            _connectionTask.SetResult(true);
            _logger.LogInformation("SignalR connection established");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start SignalR connection");
            _connectionTask.SetResult(false);
            throw;
        }
    }
    
    public async Task JoinProjectAsync(int projectId)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("JoinProjectGroup", projectId);
    }
    
    public async Task LeaveProjectAsync(int projectId)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("LeaveProjectGroup", projectId);
    }
    
    private async Task EnsureConnectedAsync()
    {
        if (!await _connectionTask.Task)
        {
            throw new InvalidOperationException("SignalR connection failed");
        }
    }
    
    // Event handlers
    private void OnTaskStatusChanged(TaskStatusChangedNotification notification)
    {
        TaskStatusChanged?.Invoke(notification);
    }
    
    private void OnTaskAssignmentChanged(TaskAssignmentChangedNotification notification)
    {
        TaskAssignmentChanged?.Invoke(notification);
    }
    
    private void OnCommentAdded(CommentAddedNotification notification)
    {
        CommentAdded?.Invoke(notification);
    }
    
    private void OnProjectTaskCountsUpdated(ProjectTaskCountsNotification notification)
    {
        ProjectTaskCountsUpdated?.Invoke(notification);
    }
    
    private async Task OnConnectionClosed(Exception exception)
    {
        _logger.LogWarning(exception, "SignalR connection closed");
        ConnectionClosed?.Invoke(exception);
    }
    
    private async Task OnReconnected(string connectionId)
    {
        _logger.LogInformation($"SignalR reconnected with connection ID: {connectionId}");
        Reconnected?.Invoke(connectionId);
    }
    
    private async Task OnReconnecting(Exception exception)
    {
        _logger.LogInformation(exception, "SignalR reconnecting...");
        Reconnecting?.Invoke(exception);
    }
    
    // Events
    public event Action<TaskStatusChangedNotification> TaskStatusChanged;
    public event Action<TaskAssignmentChangedNotification> TaskAssignmentChanged;
    public event Action<CommentAddedNotification> CommentAdded;
    public event Action<ProjectTaskCountsNotification> ProjectTaskCountsUpdated;
    public event Action<Exception> ConnectionClosed;
    public event Action<string> Reconnected;
    public event Action<Exception> Reconnecting;
    
    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
```

## PostgreSQL Integration

### Entity Framework Configuration
```csharp
public class TaskifyDbContext : DbContext
{
    public TaskifyDbContext(DbContextOptions<TaskifyDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Task> Tasks { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigureUserEntity(modelBuilder);
        ConfigureProjectEntity(modelBuilder);
        ConfigureTaskEntity(modelBuilder);
        ConfigureCommentEntity(modelBuilder);
        
        ConfigureIndexes(modelBuilder);
    }
    
    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            
            // Navigation properties
            entity.HasMany(e => e.AssignedTasks)
                  .WithOne(e => e.AssignedUser)
                  .HasForeignKey(e => e.AssignedUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasMany(e => e.Comments)
                  .WithOne(e => e.Author)
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureProjectEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Created).IsRequired();
            
            // Navigation properties
            entity.HasMany(e => e.Tasks)
                  .WithOne(e => e.Project)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureTaskEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Created).IsRequired();
            entity.Property(e => e.LastModified).IsRequired();
            
            // Convert enum to string
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            
            // Navigation properties
            entity.HasOne(e => e.Project)
                  .WithMany(e => e.Tasks)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.AssignedUser)
                  .WithMany(e => e.AssignedTasks)
                  .HasForeignKey(e => e.AssignedUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasMany(e => e.Comments)
                  .WithOne(e => e.Task)
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureCommentEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Created).IsRequired();
            
            // Navigation properties
            entity.HasOne(e => e.Task)
                  .WithMany(e => e.Comments)
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Author)
                  .WithMany(e => e.Comments)
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Task indexes for performance
        modelBuilder.Entity<Task>()
            .HasIndex(e => new { e.ProjectId, e.Status })
            .HasDatabaseName("IX_Tasks_ProjectId_Status");
        
        modelBuilder.Entity<Task>()
            .HasIndex(e => e.AssignedUserId)
            .HasDatabaseName("IX_Tasks_AssignedUserId");
        
        modelBuilder.Entity<Task>()
            .HasIndex(e => e.LastModified)
            .HasDatabaseName("IX_Tasks_LastModified");
        
        // Comment indexes
        modelBuilder.Entity<Comment>()
            .HasIndex(e => e.TaskId)
            .HasDatabaseName("IX_Comments_TaskId");
        
        modelBuilder.Entity<Comment>()
            .HasIndex(e => e.AuthorId)
            .HasDatabaseName("IX_Comments_AuthorId");
        
        modelBuilder.Entity<Comment>()
            .HasIndex(e => e.Created)
            .HasDatabaseName("IX_Comments_Created");
    }
}
```

### Database Connection Configuration
```csharp
public static class DatabaseConfiguration
{
    public static void ConfigureDatabase(this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<TaskifyDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Configure connection resilience
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                
                // Configure command timeout
                npgsqlOptions.CommandTimeout(30);
            });
            
            // Enable sensitive data logging in development
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
            
            // Configure query tracking
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        });
        
        // Configure connection pooling
        services.AddDbContextPool<TaskifyDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        }, poolSize: 128);
    }
}
```

### Migration Scripts
```sql
-- Initial migration: 001_InitialCreate.sql
CREATE TABLE "Users" (
    "Id" serial PRIMARY KEY,
    "Name" varchar(100) NOT NULL,
    "Role" varchar(50) NOT NULL
);

CREATE TABLE "Projects" (
    "Id" serial PRIMARY KEY,
    "Title" varchar(200) NOT NULL,
    "Description" varchar(1000),
    "Created" timestamp with time zone NOT NULL
);

CREATE TABLE "Tasks" (
    "Id" serial PRIMARY KEY,
    "Title" varchar(200) NOT NULL,
    "Description" varchar(2000),
    "Status" varchar(20) NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    "LastModified" timestamp with time zone NOT NULL,
    "ProjectId" integer NOT NULL REFERENCES "Projects"("Id") ON DELETE CASCADE,
    "AssignedUserId" integer REFERENCES "Users"("Id") ON DELETE SET NULL
);

CREATE TABLE "Comments" (
    "Id" serial PRIMARY KEY,
    "Content" varchar(2000) NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    "LastModified" timestamp with time zone,
    "TaskId" integer NOT NULL REFERENCES "Tasks"("Id") ON DELETE CASCADE,
    "AuthorId" integer NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX "IX_Tasks_ProjectId_Status" ON "Tasks" ("ProjectId", "Status");
CREATE INDEX "IX_Tasks_AssignedUserId" ON "Tasks" ("AssignedUserId");
CREATE INDEX "IX_Tasks_LastModified" ON "Tasks" ("LastModified");
CREATE INDEX "IX_Comments_TaskId" ON "Comments" ("TaskId");
CREATE INDEX "IX_Comments_AuthorId" ON "Comments" ("AuthorId");
CREATE INDEX "IX_Comments_Created" ON "Comments" ("Created");
```

### Database Seeding
```csharp
public class DatabaseSeeder
{
    private readonly TaskifyDbContext _context;
    
    public DatabaseSeeder(TaskifyDbContext context)
    {
        _context = context;
    }
    
    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        
        if (!await _context.Users.AnyAsync())
        {
            await SeedUsersAsync();
        }
        
        if (!await _context.Projects.AnyAsync())
        {
            await SeedProjectsAsync();
        }
        
        if (!await _context.Tasks.AnyAsync())
        {
            await SeedTasksAsync();
        }
        
        if (!await _context.Comments.AnyAsync())
        {
            await SeedCommentsAsync();
        }
    }
    
    private async Task SeedUsersAsync()
    {
        var users = new[]
        {
            new User { Name = "Sarah Chen", Role = "Product Manager" },
            new User { Name = "Alex Rodriguez", Role = "Senior Engineer" },
            new User { Name = "Jordan Kim", Role = "Engineer" },
            new User { Name = "Taylor Swift", Role = "Engineer" },
            new User { Name = "Morgan Davis", Role = "Engineer" }
        };
        
        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedProjectsAsync()
    {
        var projects = new[]
        {
            new Project 
            { 
                Title = "Mobile App Redesign", 
                Description = "Complete redesign of mobile application",
                Created = DateTime.UtcNow.AddDays(-30)
            },
            new Project 
            { 
                Title = "API Integration Platform", 
                Description = "Build integration platform for third-party APIs",
                Created = DateTime.UtcNow.AddDays(-60)
            },
            new Project 
            { 
                Title = "Team Onboarding System", 
                Description = "System for onboarding new team members",
                Created = DateTime.UtcNow.AddDays(-90)
            }
        };
        
        _context.Projects.AddRange(projects);
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedTasksAsync()
    {
        var users = await _context.Users.ToListAsync();
        var projects = await _context.Projects.ToListAsync();
        
        var tasks = new List<Task>();
        
        // Mobile App Redesign tasks (21 total)
        tasks.AddRange(CreateProjectTasks(projects[0], users, GetMobileAppTasks()));
        
        // API Integration Platform tasks (20 total)
        tasks.AddRange(CreateProjectTasks(projects[1], users, GetAPIIntegrationTasks()));
        
        // Team Onboarding System tasks (20 total)
        tasks.AddRange(CreateProjectTasks(projects[2], users, GetOnboardingTasks()));
        
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();
    }
    
    private List<Task> CreateProjectTasks(Project project, List<User> users, List<(string Title, string Description, TaskStatus Status)> taskData)
    {
        var tasks = new List<Task>();
        var random = new Random();
        
        foreach (var (title, description, status) in taskData)
        {
            var assignedUser = users[random.Next(users.Count)];
            var created = DateTime.UtcNow.AddDays(-random.Next(1, 30));
            
            tasks.Add(new Task
            {
                Title = title,
                Description = description,
                Status = status,
                ProjectId = project.Id,
                AssignedUserId = assignedUser.Id,
                Created = created,
                LastModified = created.AddHours(random.Next(0, 48))
            });
        }
        
        return tasks;
    }
    
    // Task data methods would be implemented here...
}
```

## Performance Optimizations

### Connection Pooling
```csharp
public class DatabaseConnectionPool
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _semaphore;
    private readonly Queue<NpgsqlConnection> _connections;
    
    public DatabaseConnectionPool(string connectionString, int maxConnections = 10)
    {
        _connectionString = connectionString;
        _semaphore = new SemaphoreSlim(maxConnections, maxConnections);
        _connections = new Queue<NpgsqlConnection>();
    }
    
    public async Task<NpgsqlConnection> GetConnectionAsync()
    {
        await _semaphore.WaitAsync();
        
        if (_connections.TryDequeue(out var connection) && connection.State == ConnectionState.Open)
        {
            return connection;
        }
        
        connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
    
    public void ReturnConnection(NpgsqlConnection connection)
    {
        if (connection.State == ConnectionState.Open)
        {
            _connections.Enqueue(connection);
        }
        else
        {
            connection.Dispose();
        }
        
        _semaphore.Release();
    }
}
```

### Query Optimization
```csharp
public class OptimizedTaskQueries
{
    private readonly TaskifyDbContext _context;
    
    public OptimizedTaskQueries(TaskifyDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Task>> GetTasksByProjectAsync(int projectId)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.AssignedUser)
            .Include(t => t.Comments.OrderBy(c => c.Created).Take(5)) // Limit comments
            .OrderBy(t => t.Status)
            .ThenBy(t => t.Created)
            .AsNoTracking() // Read-only query
            .ToListAsync();
    }
    
    public async Task<Dictionary<TaskStatus, int>> GetTaskCountsByProjectAsync(int projectId)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }
}
```