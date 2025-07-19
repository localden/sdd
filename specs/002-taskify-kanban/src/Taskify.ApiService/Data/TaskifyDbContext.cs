using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Taskify.ApiService.Models;

namespace Taskify.ApiService.Data;

public class TaskifyDbContext : DbContext
{
    public TaskifyDbContext(DbContextOptions<TaskifyDbContext> options) : base(options)
    {
    }
    
    // Core entities
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    // Enhanced kanban entities
    public DbSet<KanbanBoard> KanbanBoards { get; set; }
    public DbSet<BoardColumn> BoardColumns { get; set; }
    public DbSet<TaskPosition> TaskPositions { get; set; }
    public DbSet<BoardFilter> BoardFilters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigureCoreEntities(modelBuilder);
        ConfigureKanbanEntities(modelBuilder);
        ConfigureEnhancedIndexes(modelBuilder);
        ConfigureJsonColumns(modelBuilder);
        ConfigureCascadeDeletes(modelBuilder);
    }

    private void ConfigureCoreEntities(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasOne(e => e.Owner)
                  .WithMany(u => u.OwnedProjects)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ProjectMember configuration
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("NOW()");
            
            entity.HasIndex(e => new { e.ProjectId, e.UserId }).IsUnique();
            
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Members)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.User)
                  .WithMany(u => u.ProjectMemberships)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskItem configuration
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Priority).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExternalId).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasOne(e => e.Assignee)
                  .WithMany(u => u.AssignedTasks)
                  .HasForeignKey(e => e.AssigneeId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.ActionUrl).HasMaxLength(500);
            entity.Property(e => e.Priority).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Notifications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.RelatedTask)
                  .WithMany(t => t.Notifications)
                  .HasForeignKey(e => e.RelatedTaskId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.RelatedBoard)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedBoardId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureKanbanEntities(ModelBuilder modelBuilder)
    {
        // KanbanBoard configuration
        modelBuilder.Entity<KanbanBoard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.KanbanBoards)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // BoardColumn configuration
        modelBuilder.Entity<BoardColumn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.KeyField).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasOne(e => e.Board)
                  .WithMany(b => b.Columns)
                  .HasForeignKey(e => e.BoardId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskPosition configuration
        modelBuilder.Entity<TaskPosition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Position).HasPrecision(10, 5);
            entity.Property(e => e.SwimlaneValue).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            
            // Optimistic concurrency
            entity.Property(e => e.Version).IsConcurrencyToken();
            
            // Unique constraint: one position per task per board
            entity.HasIndex(e => new { e.TaskId, e.BoardId }).IsUnique();
            
            entity.HasOne(e => e.Task)
                  .WithMany(t => t.Positions)
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Board)
                  .WithMany(b => b.TaskPositions)
                  .HasForeignKey(e => e.BoardId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Column)
                  .WithMany(c => c.TaskPositions)
                  .HasForeignKey(e => e.ColumnId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.UpdatedByUser)
                  .WithMany(u => u.TaskPositionUpdates)
                  .HasForeignKey(e => e.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // BoardFilter configuration
        modelBuilder.Entity<BoardFilter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasOne(e => e.Board)
                  .WithMany(b => b.Filters)
                  .HasForeignKey(e => e.BoardId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Creator)
                  .WithMany(u => u.CreatedFilters)
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureEnhancedIndexes(ModelBuilder modelBuilder)
    {
        // Performance indexes for kanban operations
        modelBuilder.Entity<TaskPosition>()
            .HasIndex(e => new { e.BoardId, e.ColumnId, e.Position })
            .HasDatabaseName("IX_TaskPositions_Board_Column_Position");

        modelBuilder.Entity<TaskPosition>()
            .HasIndex(e => e.UpdatedAt)
            .HasDatabaseName("IX_TaskPositions_UpdatedAt");

        modelBuilder.Entity<BoardColumn>()
            .HasIndex(e => new { e.BoardId, e.Position })
            .HasDatabaseName("IX_BoardColumns_Board_Position");

        modelBuilder.Entity<KanbanBoard>()
            .HasIndex(e => e.ProjectId)
            .HasDatabaseName("IX_KanbanBoards_ProjectId");

        // Enhanced TaskItem indexes for kanban queries
        modelBuilder.Entity<TaskItem>()
            .HasIndex(e => new { e.ProjectId, e.Status, e.Priority })
            .HasDatabaseName("IX_Tasks_Project_Status_Priority");

        modelBuilder.Entity<TaskItem>()
            .HasIndex(e => e.DueDate)
            .HasDatabaseName("IX_Tasks_DueDate");

        // Notification indexes
        modelBuilder.Entity<Notification>()
            .HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt })
            .HasDatabaseName("IX_Notifications_User_Read_Created");
    }

    private void ConfigureJsonColumns(ModelBuilder modelBuilder)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // KanbanBoard settings JSON column
        modelBuilder.Entity<KanbanBoard>()
            .Property(e => e.Settings)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<KanbanBoardSettings>(v, jsonOptions) ?? new()
            )
            .HasColumnType("jsonb");

        // TaskItem tags and metadata JSON columns
        modelBuilder.Entity<TaskItem>()
            .Property(e => e.Tags)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new()
            )
            .HasColumnType("jsonb");

        modelBuilder.Entity<TaskItem>()
            .Property(e => e.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<TaskMetadata>(v, jsonOptions) ?? new()
            )
            .HasColumnType("jsonb");

        // BoardFilter criteria JSON column
        modelBuilder.Entity<BoardFilter>()
            .Property(e => e.FilterCriteria)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<BoardFilterCriteria>(v, jsonOptions) ?? new()
            )
            .HasColumnType("jsonb");
    }

    private void ConfigureCascadeDeletes(ModelBuilder modelBuilder)
    {
        // Ensure proper cascade behavior for kanban entities
        modelBuilder.Entity<KanbanBoard>()
            .HasMany(b => b.Columns)
            .WithOne(c => c.Board)
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<KanbanBoard>()
            .HasMany(b => b.TaskPositions)
            .WithOne(tp => tp.Board)
            .HasForeignKey(tp => tp.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<KanbanBoard>()
            .HasMany(b => b.Filters)
            .WithOne(f => f.Board)
            .HasForeignKey(f => f.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Property("UpdatedAt").Metadata != null)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Added && entry.Property("CreatedAt").Metadata != null)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
    }
}