using System.ComponentModel.DataAnnotations;

namespace Taskify.ApiService.Models;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty; // task_assigned, task_moved, board_updated, etc.
    
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public Guid? RelatedTaskId { get; set; }
    public Guid? RelatedBoardId { get; set; }
    
    [MaxLength(500)]
    public string? ActionUrl { get; set; }
    
    [MaxLength(50)]
    public string Priority { get; set; } = "normal"; // low, normal, high, urgent
    
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public TaskItem? RelatedTask { get; set; }
    public KanbanBoard? RelatedBoard { get; set; }
}