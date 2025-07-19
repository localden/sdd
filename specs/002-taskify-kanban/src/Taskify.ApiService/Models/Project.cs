using System.ComponentModel.DataAnnotations;

namespace Taskify.ApiService.Models;

public class Project
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<KanbanBoard> KanbanBoards { get; set; } = new List<KanbanBoard>();
}

public class ProjectMember
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Member"; // Owner, Admin, Member, Viewer
    
    public DateTime JoinedAt { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}