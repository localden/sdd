# Data Model: Enhanced Kanban Board System

**Document**: 002-taskify-kanban/implementation-details/02-data-model.md  
**Created**: 2025-07-19  
**Status**: Enhanced database schema for kanban features  

---

## Overview

This document defines the enhanced database schema and Entity Framework Core models required for implementing advanced kanban board functionality. The design extends the existing Taskify data model while maintaining backward compatibility and optimizing for real-time collaboration and large board performance.

---

## Enhanced Entity Relationship Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│     User        │    │    Project      │    │   KanbanBoard   │
│                 │    │                 │    │                 │
│ Id (PK)         │    │ Id (PK)         │    │ Id (PK)         │
│ Email           │    │ Name            │    │ ProjectId (FK)  │
│ Name            │    │ Description     │    │ Name            │
│ PasswordHash    │    │ CreatedAt       │    │ Description     │
│ CreatedAt       │◄───┤ OwnerId (FK)    │◄───┤ Settings (JSON) │
│                 │    │                 │    │ CreatedAt       │
└─────────────────┘    └─────────────────┘    │ UpdatedAt       │
                                              └─────────────────┘
                                                       │
                                                       │ 1:N
                                                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   TaskItem      │    │  TaskPosition   │    │  BoardColumn    │
│                 │    │                 │    │                 │
│ Id (PK)         │    │ Id (PK)         │    │ Id (PK)         │
│ Title           │    │ TaskId (FK)     │────┤ BoardId (FK)    │
│ Description     │◄───┤ BoardId (FK)    │    │ Name            │
│ Status          │    │ ColumnId (FK)   │────┤ KeyField        │
│ Priority        │    │ Position        │    │ Position        │
│ DueDate         │    │ SwimlaneValue   │    │ WipLimit        │
│ AssigneeId (FK) │    │ Version         │    │ Color           │
│ ProjectId (FK)  │    │ UpdatedAt       │    │ CreatedAt       │
│ CreatedAt       │    │                 │    │                 │
│ UpdatedAt       │    └─────────────────┘    └─────────────────┘
└─────────────────┘
         │
         │ 1:N
         ▼
┌─────────────────┐    ┌─────────────────┐
│  Notification   │    │   BoardFilter   │
│                 │    │                 │
│ Id (PK)         │    │ Id (PK)         │
│ UserId (FK)     │    │ BoardId (FK)    │
│ Type            │    │ Name            │
│ Title           │    │ FilterCriteria  │
│ Message         │    │ IsDefault       │
│ IsRead          │    │ CreatedBy (FK)  │
│ CreatedAt       │    │ CreatedAt       │
│                 │    │                 │
└─────────────────┘    └─────────────────┘
```

---

## Enhanced Entity Definitions

### KanbanBoard Entity
```csharp
public class KanbanBoard
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public KanbanBoardSettings Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public ICollection<BoardColumn> Columns { get; set; } = new List<BoardColumn>();
    public ICollection<TaskPosition> TaskPositions { get; set; } = new List<TaskPosition>();
    public ICollection<BoardFilter> Filters { get; set; } = new List<BoardFilter>();
}

public class KanbanBoardSettings
{
    public bool EnableWipLimits { get; set; } = true;
    public bool EnableSwimlanes { get; set; } = false;
    public string DefaultSwimlaneBy { get; set; } = "assignee"; // assignee, priority, project
    public bool EnableRealTimeUpdates { get; set; } = true;
    public int MaxTasksPerColumn { get; set; } = 100;
    public string Theme { get; set; } = "default";
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}
```

### BoardColumn Entity
```csharp
public class BoardColumn
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyField { get; set; } = string.Empty; // Maps to TaskItem.Status values
    public int Position { get; set; }
    public int? WipLimit { get; set; }
    public string? Color { get; set; } // Hex color code
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public KanbanBoard Board { get; set; } = null!;
    public ICollection<TaskPosition> TaskPositions { get; set; } = new List<TaskPosition>();
}
```

### TaskPosition Entity
```csharp
public class TaskPosition
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public decimal Position { get; set; } // Decimal for fractional positioning between tasks
    public string? SwimlaneValue { get; set; } // Value for swimlane grouping
    public int Version { get; set; } = 1; // For optimistic concurrency control
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }

    // Navigation properties
    public TaskItem Task { get; set; } = null!;
    public KanbanBoard Board { get; set; } = null!;
    public BoardColumn Column { get; set; } = null!;
    public User UpdatedByUser { get; set; } = null!;
}
```

### BoardFilter Entity
```csharp
public class BoardFilter
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public BoardFilterCriteria FilterCriteria { get; set; } = new();
    public bool IsDefault { get; set; } = false;
    public bool IsShared { get; set; } = false;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public KanbanBoard Board { get; set; } = null!;
    public User Creator { get; set; } = null!;
}

public class BoardFilterCriteria
{
    public List<string>? Statuses { get; set; }
    public List<Guid>? AssigneeIds { get; set; }
    public List<string>? Priorities { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public List<string>? Tags { get; set; }
    public string? SearchText { get; set; }
    public Dictionary<string, object> CustomCriteria { get; set; } = new();
}
```

### Enhanced TaskItem Entity
```csharp
// Enhanced existing TaskItem entity with kanban-specific properties
public class TaskItem
{
    // Existing properties...
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Enhanced properties for kanban
    public List<string> Tags { get; set; } = new();
    public TaskMetadata Metadata { get; set; } = new();
    public int EstimatedHours { get; set; }
    public string? ExternalId { get; set; } // For integrations

    // Navigation properties
    public User? Assignee { get; set; }
    public Project Project { get; set; } = null!;
    public ICollection<TaskPosition> Positions { get; set; } = new List<TaskPosition>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

public class TaskMetadata
{
    public string? Color { get; set; }
    public Dictionary<string, string> CustomFields { get; set; } = new();
    public DateTime? LastMovedAt { get; set; }
    public Guid? LastMovedBy { get; set; }
    public int MoveCount { get; set; } = 0;
}
```

---

## Enhanced Entity Framework Configuration

### TaskifyDbContext Enhancements
```csharp
public class TaskifyDbContext : DbContext
{
    // Existing DbSets...
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    // Enhanced DbSets for kanban features
    public DbSet<KanbanBoard> KanbanBoards { get; set; }
    public DbSet<BoardColumn> BoardColumns { get; set; }
    public DbSet<TaskPosition> TaskPositions { get; set; }
    public DbSet<BoardFilter> BoardFilters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigureKanbanEntities(modelBuilder);
        ConfigureEnhancedIndexes(modelBuilder);
        ConfigureJsonColumns(modelBuilder);
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
            
            // JSON column for settings
            entity.Property(e => e.Settings)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<KanbanBoardSettings>(v, (JsonSerializerOptions?)null) ?? new());
            
            // Relationships
            entity.HasOne(e => e.Project)
                  .WithMany()
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // BoardColumn configuration
        modelBuilder.Entity<BoardColumn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.KeyField).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(7); // Hex color
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            
            // Relationships
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
            entity.HasIndex(e => new { e.TaskId, e.BoardId })
                  .IsUnique();
            
            // Relationships
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
                  .WithMany()
                  .HasForeignKey(e => e.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // BoardFilter configuration
        modelBuilder.Entity<BoardFilter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            
            // JSON column for filter criteria
            entity.Property(e => e.FilterCriteria)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<BoardFilterCriteria>(v, (JsonSerializerOptions?)null) ?? new());
            
            // Relationships
            entity.HasOne(e => e.Board)
                  .WithMany(b => b.Filters)
                  .HasForeignKey(e => e.BoardId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Creator)
                  .WithMany()
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
    }

    private void ConfigureJsonColumns(ModelBuilder modelBuilder)
    {
        // Enhanced TaskItem metadata
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(e => e.Tags)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());

            entity.Property(e => e.Metadata)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<TaskMetadata>(v, (JsonSerializerOptions?)null) ?? new());
        });
    }
}
```

---

## Database Migration Scripts

### Initial Kanban Migration
```sql
-- Migration: 20250719_AddKanbanBoardFeatures

-- Create kanban_boards table
CREATE TABLE kanban_boards (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(1000),
    settings JSONB NOT NULL DEFAULT '{}',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT FK_kanban_boards_project_id FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE
);

-- Create board_columns table
CREATE TABLE board_columns (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    board_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    key_field VARCHAR(100) NOT NULL,
    position INTEGER NOT NULL,
    wip_limit INTEGER,
    color VARCHAR(7),
    is_visible BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT FK_board_columns_board_id FOREIGN KEY (board_id) REFERENCES kanban_boards(id) ON DELETE CASCADE
);

-- Create task_positions table
CREATE TABLE task_positions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    task_id UUID NOT NULL,
    board_id UUID NOT NULL,
    column_id UUID NOT NULL,
    position DECIMAL(10,5) NOT NULL,
    swimlane_value VARCHAR(255),
    version INTEGER NOT NULL DEFAULT 1,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_by UUID NOT NULL,
    CONSTRAINT FK_task_positions_task_id FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE CASCADE,
    CONSTRAINT FK_task_positions_board_id FOREIGN KEY (board_id) REFERENCES kanban_boards(id) ON DELETE CASCADE,
    CONSTRAINT FK_task_positions_column_id FOREIGN KEY (column_id) REFERENCES board_columns(id) ON DELETE RESTRICT,
    CONSTRAINT FK_task_positions_updated_by FOREIGN KEY (updated_by) REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT UQ_task_positions_task_board UNIQUE (task_id, board_id)
);

-- Create board_filters table
CREATE TABLE board_filters (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    board_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    filter_criteria JSONB NOT NULL DEFAULT '{}',
    is_default BOOLEAN NOT NULL DEFAULT false,
    is_shared BOOLEAN NOT NULL DEFAULT false,
    created_by UUID NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT FK_board_filters_board_id FOREIGN KEY (board_id) REFERENCES kanban_boards(id) ON DELETE CASCADE,
    CONSTRAINT FK_board_filters_created_by FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT
);

-- Enhance tasks table for kanban features
ALTER TABLE tasks ADD COLUMN tags JSONB DEFAULT '[]';
ALTER TABLE tasks ADD COLUMN metadata JSONB DEFAULT '{}';
ALTER TABLE tasks ADD COLUMN estimated_hours INTEGER DEFAULT 0;
ALTER TABLE tasks ADD COLUMN external_id VARCHAR(255);

-- Create performance indexes
CREATE INDEX IX_TaskPositions_Board_Column_Position ON task_positions(board_id, column_id, position);
CREATE INDEX IX_TaskPositions_UpdatedAt ON task_positions(updated_at);
CREATE INDEX IX_BoardColumns_Board_Position ON board_columns(board_id, position);
CREATE INDEX IX_KanbanBoards_ProjectId ON kanban_boards(project_id);
CREATE INDEX IX_Tasks_Project_Status_Priority ON tasks(project_id, status, priority);

-- Create trigger for updating updated_at timestamps
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER update_kanban_boards_updated_at
    BEFORE UPDATE ON kanban_boards
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_task_positions_updated_at
    BEFORE UPDATE ON task_positions
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

---

## Data Seeding for Enhanced Features

### Enhanced DbContext Seeding
```csharp
public static class KanbanDataSeeder
{
    public static void SeedKanbanData(TaskifyDbContext context)
    {
        if (context.KanbanBoards.Any())
            return; // Already seeded

        var projects = context.Projects.ToList();
        if (!projects.Any())
            return; // Need projects first

        foreach (var project in projects.Take(2)) // Seed for first 2 projects
        {
            var board = new KanbanBoard
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Name = $"{project.Name} Kanban Board",
                Description = $"Main kanban board for {project.Name}",
                Settings = new KanbanBoardSettings
                {
                    EnableWipLimits = true,
                    EnableSwimlanes = true,
                    DefaultSwimlaneBy = "assignee",
                    EnableRealTimeUpdates = true,
                    MaxTasksPerColumn = 50
                }
            };

            context.KanbanBoards.Add(board);
            context.SaveChanges();

            // Create default columns
            var columns = new[]
            {
                new BoardColumn { Id = Guid.NewGuid(), BoardId = board.Id, Name = "To Do", KeyField = "todo", Position = 1, WipLimit = 10, Color = "#FF6B6B" },
                new BoardColumn { Id = Guid.NewGuid(), BoardId = board.Id, Name = "In Progress", KeyField = "inprogress", Position = 2, WipLimit = 5, Color = "#4ECDC4" },
                new BoardColumn { Id = Guid.NewGuid(), BoardId = board.Id, Name = "Review", KeyField = "review", Position = 3, WipLimit = 3, Color = "#45B7D1" },
                new BoardColumn { Id = Guid.NewGuid(), BoardId = board.Id, Name = "Done", KeyField = "done", Position = 4, Color = "#96CEB4" }
            };

            context.BoardColumns.AddRange(columns);
            context.SaveChanges();

            // Position existing tasks on the board
            var tasks = context.Tasks.Where(t => t.ProjectId == project.Id).ToList();
            var taskPositions = new List<TaskPosition>();

            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                var column = columns[i % columns.Length]; // Distribute tasks across columns

                taskPositions.Add(new TaskPosition
                {
                    Id = Guid.NewGuid(),
                    TaskId = task.Id,
                    BoardId = board.Id,
                    ColumnId = column.Id,
                    Position = (i / columns.Length) + 1,
                    SwimlaneValue = task.AssigneeId?.ToString(),
                    UpdatedBy = project.OwnerId
                });
            }

            context.TaskPositions.AddRange(taskPositions);

            // Create default filters
            var defaultFilter = new BoardFilter
            {
                Id = Guid.NewGuid(),
                BoardId = board.Id,
                Name = "All Tasks",
                FilterCriteria = new BoardFilterCriteria(),
                IsDefault = true,
                IsShared = true,
                CreatedBy = project.OwnerId
            };

            var highPriorityFilter = new BoardFilter
            {
                Id = Guid.NewGuid(),
                BoardId = board.Id,
                Name = "High Priority",
                FilterCriteria = new BoardFilterCriteria
                {
                    Priorities = new List<string> { "High" }
                },
                IsDefault = false,
                IsShared = true,
                CreatedBy = project.OwnerId
            };

            context.BoardFilters.AddRange(defaultFilter, highPriorityFilter);
        }

        context.SaveChanges();
    }
}
```

---

## Data Model Status: COMPLETE ✅

All enhanced data model requirements have been defined:
- [x] Enhanced entity relationship diagram with kanban entities
- [x] KanbanBoard, BoardColumn, TaskPosition, and BoardFilter entity definitions
- [x] Enhanced TaskItem entity with kanban-specific properties
- [x] Comprehensive Entity Framework configuration with indexes and relationships
- [x] Database migration scripts for kanban features
- [x] JSON column configurations for flexible settings and metadata
- [x] Optimistic concurrency control for real-time collaboration
- [x] Performance-optimized indexes for large board scenarios
- [x] Data seeding strategy for enhanced features

**Ready to proceed to enhanced API contracts definition**