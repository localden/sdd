using System.ComponentModel.DataAnnotations;

namespace Taskify.ApiService.Models;

public class User
{
    public Guid Id { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskPosition> TaskPositionUpdates { get; set; } = new List<TaskPosition>();
    public ICollection<BoardFilter> CreatedFilters { get; set; } = new List<BoardFilter>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}