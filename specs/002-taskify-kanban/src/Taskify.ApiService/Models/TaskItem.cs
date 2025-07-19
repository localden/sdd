using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Taskify.ApiService.Models;

public class TaskItem
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "todo"; // todo, inprogress, review, done
    
    [Required]
    [MaxLength(50)]
    public string Priority { get; set; } = "medium"; // low, medium, high
    
    public DateTime? DueDate { get; set; }
    
    public Guid? AssigneeId { get; set; }
    public Guid ProjectId { get; set; }
    
    // Enhanced properties for kanban
    public List<string> Tags { get; set; } = new();
    public TaskMetadata Metadata { get; set; } = new();
    public int EstimatedHours { get; set; }
    
    [MaxLength(255)]
    public string? ExternalId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public User? Assignee { get; set; }
    public Project Project { get; set; } = null!;
    public ICollection<TaskPosition> Positions { get; set; } = new List<TaskPosition>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

public class TaskMetadata
{
    [MaxLength(7)]
    public string? Color { get; set; }
    
    public Dictionary<string, string> CustomFields { get; set; } = new();
    public DateTime? LastMovedAt { get; set; }
    public Guid? LastMovedBy { get; set; }
    public int MoveCount { get; set; } = 0;
}