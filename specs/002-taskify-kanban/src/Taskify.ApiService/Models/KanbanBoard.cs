using System.ComponentModel.DataAnnotations;

namespace Taskify.ApiService.Models;

public class KanbanBoard
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
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
    
    [MaxLength(50)]
    public string DefaultSwimlaneBy { get; set; } = "assignee"; // assignee, priority, project
    
    public bool EnableRealTimeUpdates { get; set; } = true;
    public int MaxTasksPerColumn { get; set; } = 100;
    
    [MaxLength(50)]
    public string Theme { get; set; } = "default";
    
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

public class BoardColumn
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string KeyField { get; set; } = string.Empty; // Maps to TaskItem.Status values
    
    public int Position { get; set; }
    public int? WipLimit { get; set; }
    
    [MaxLength(7)]
    public string? Color { get; set; } // Hex color code
    
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public KanbanBoard Board { get; set; } = null!;
    public ICollection<TaskPosition> TaskPositions { get; set; } = new List<TaskPosition>();
}

public class TaskPosition
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    
    public decimal Position { get; set; } // Decimal for fractional positioning between tasks
    
    [MaxLength(255)]
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

public class BoardFilter
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    
    [Required]
    [MaxLength(255)]
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
    
    [MaxLength(255)]
    public string? SearchText { get; set; }
    
    public Dictionary<string, object> CustomCriteria { get; set; } = new();
}