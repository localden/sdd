using Microsoft.EntityFrameworkCore;
using Taskify.ApiService.Models;

namespace Taskify.ApiService.Data;

public static class TaskifyDataSeeder
{
    public static async Task SeedAsync(TaskifyDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            return; // Database already seeded
        }

        await SeedUsersAsync(context);
        await SeedProjectsAsync(context);
        await SeedTasksAsync(context);
        await SeedKanbanBoardsAsync(context);
        
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(TaskifyDbContext context)
    {
        var users = new[]
        {
            new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Email = "john.doe@example.com",
                Name = "John Doe",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Email = "jane.smith@example.com",
                Name = "Jane Smith",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Email = "mike.johnson@example.com",
                Name = "Mike Johnson",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Email = "sarah.wilson@example.com",
                Name = "Sarah Wilson",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProjectsAsync(TaskifyDbContext context)
    {
        var projects = new[]
        {
            new Project
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Website Redesign",
                Description = "Complete redesign of the company website with modern UI/UX",
                OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Project
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Mobile App Development",
                Description = "Native mobile application for iOS and Android platforms",
                OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Project
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Marketing Campaign Q4",
                Description = "Digital marketing campaign for Q4 product launch",
                OwnerId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();

        // Add project members
        var projectMembers = new[]
        {
            // Website Redesign project members
            new ProjectMember { Id = Guid.NewGuid(), ProjectId = projects[0].Id, UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), Role = "Developer", JoinedAt = DateTime.UtcNow },
            new ProjectMember { Id = Guid.NewGuid(), ProjectId = projects[0].Id, UserId = Guid.Parse("44444444-4444-4444-4444-444444444444"), Role = "Designer", JoinedAt = DateTime.UtcNow },
            
            // Mobile App project members
            new ProjectMember { Id = Guid.NewGuid(), ProjectId = projects[1].Id, UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), Role = "Developer", JoinedAt = DateTime.UtcNow },
            new ProjectMember { Id = Guid.NewGuid(), ProjectId = projects[1].Id, UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), Role = "QA", JoinedAt = DateTime.UtcNow },
            
            // Marketing Campaign project members
            new ProjectMember { Id = Guid.NewGuid(), ProjectId = projects[2].Id, UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), Role = "Content Creator", JoinedAt = DateTime.UtcNow },
            new ProjectMember { Id = Guid.NewGuid(), ProjectId = projects[2].Id, UserId = Guid.Parse("44444444-4444-4444-4444-444444444444"), Role = "Designer", JoinedAt = DateTime.UtcNow }
        };

        context.ProjectMembers.AddRange(projectMembers);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTasksAsync(TaskifyDbContext context)
    {
        var tasks = new[]
        {
            // Website Redesign tasks
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Create wireframes for homepage",
                Description = "Design initial wireframes and user flow for the new homepage layout",
                Status = "todo",
                Priority = "high",
                DueDate = DateTime.UtcNow.AddDays(7),
                AssigneeId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                ProjectId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Tags = new() { "design", "wireframes", "homepage" },
                EstimatedHours = 8,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Set up development environment",
                Description = "Configure local development environment with latest frameworks",
                Status = "inprogress",
                Priority = "medium",
                DueDate = DateTime.UtcNow.AddDays(3),
                AssigneeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ProjectId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Tags = new() { "development", "setup", "environment" },
                EstimatedHours = 4,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Conduct user research",
                Description = "Interview existing users about current website pain points",
                Status = "review",
                Priority = "high",
                DueDate = DateTime.UtcNow.AddDays(5),
                AssigneeId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ProjectId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Tags = new() { "research", "users", "interviews" },
                EstimatedHours = 12,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            
            // Mobile App tasks
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Design app icon and branding",
                Description = "Create app icon, splash screen, and establish visual branding guidelines",
                Status = "todo",
                Priority = "medium",
                DueDate = DateTime.UtcNow.AddDays(10),
                AssigneeId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                ProjectId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Tags = new() { "design", "branding", "icon" },
                EstimatedHours = 16,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Implement user authentication",
                Description = "Build secure login/signup flow with OAuth integration",
                Status = "inprogress",
                Priority = "high",
                DueDate = DateTime.UtcNow.AddDays(14),
                AssigneeId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ProjectId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Tags = new() { "development", "authentication", "security" },
                EstimatedHours = 20,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            
            // Marketing Campaign tasks
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Create social media content calendar",
                Description = "Plan and schedule social media posts for Q4 campaign",
                Status = "todo",
                Priority = "medium",
                DueDate = DateTime.UtcNow.AddDays(21),
                AssigneeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ProjectId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Tags = new() { "content", "social-media", "calendar" },
                EstimatedHours = 6,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Design campaign graphics",
                Description = "Create banner ads, social media graphics, and email templates",
                Status = "inprogress",
                Priority = "high",
                DueDate = DateTime.UtcNow.AddDays(14),
                AssigneeId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                ProjectId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Tags = new() { "design", "graphics", "campaign" },
                EstimatedHours = 24,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Tasks.AddRange(tasks);
        await context.SaveChangesAsync();
    }

    private static async Task SeedKanbanBoardsAsync(TaskifyDbContext context)
    {
        var projects = await context.Projects.ToListAsync();
        
        foreach (var project in projects.Take(2)) // Create boards for first 2 projects
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
                    MaxTasksPerColumn = 50,
                    Theme = "default"
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.KanbanBoards.Add(board);
            await context.SaveChangesAsync();

            // Create default columns
            var columns = new[]
            {
                new BoardColumn 
                { 
                    Id = Guid.NewGuid(), 
                    BoardId = board.Id, 
                    Name = "To Do", 
                    KeyField = "todo", 
                    Position = 1, 
                    WipLimit = 10, 
                    Color = "#FF6B6B",
                    CreatedAt = DateTime.UtcNow
                },
                new BoardColumn 
                { 
                    Id = Guid.NewGuid(), 
                    BoardId = board.Id, 
                    Name = "In Progress", 
                    KeyField = "inprogress", 
                    Position = 2, 
                    WipLimit = 5, 
                    Color = "#4ECDC4",
                    CreatedAt = DateTime.UtcNow
                },
                new BoardColumn 
                { 
                    Id = Guid.NewGuid(), 
                    BoardId = board.Id, 
                    Name = "Review", 
                    KeyField = "review", 
                    Position = 3, 
                    WipLimit = 3, 
                    Color = "#45B7D1",
                    CreatedAt = DateTime.UtcNow
                },
                new BoardColumn 
                { 
                    Id = Guid.NewGuid(), 
                    BoardId = board.Id, 
                    Name = "Done", 
                    KeyField = "done", 
                    Position = 4, 
                    Color = "#96CEB4",
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.BoardColumns.AddRange(columns);
            await context.SaveChangesAsync();

            // Position existing tasks on the board
            var tasks = await context.Tasks
                .Where(t => t.ProjectId == project.Id)
                .ToListAsync();

            var taskPositions = new List<TaskPosition>();

            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                var column = columns.FirstOrDefault(c => c.KeyField == task.Status) ?? columns[0];

                taskPositions.Add(new TaskPosition
                {
                    Id = Guid.NewGuid(),
                    TaskId = task.Id,
                    BoardId = board.Id,
                    ColumnId = column.Id,
                    Position = (decimal)(i + 1),
                    SwimlaneValue = task.AssigneeId?.ToString(),
                    UpdatedBy = project.OwnerId,
                    UpdatedAt = DateTime.UtcNow
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
                CreatedBy = project.OwnerId,
                CreatedAt = DateTime.UtcNow
            };

            var highPriorityFilter = new BoardFilter
            {
                Id = Guid.NewGuid(),
                BoardId = board.Id,
                Name = "High Priority",
                FilterCriteria = new BoardFilterCriteria
                {
                    Priorities = new List<string> { "high" }
                },
                IsDefault = false,
                IsShared = true,
                CreatedBy = project.OwnerId,
                CreatedAt = DateTime.UtcNow
            };

            var myTasksFilter = new BoardFilter
            {
                Id = Guid.NewGuid(),
                BoardId = board.Id,
                Name = "My Tasks",
                FilterCriteria = new BoardFilterCriteria
                {
                    AssigneeIds = new List<Guid> { project.OwnerId }
                },
                IsDefault = false,
                IsShared = false,
                CreatedBy = project.OwnerId,
                CreatedAt = DateTime.UtcNow
            };

            context.BoardFilters.AddRange(defaultFilter, highPriorityFilter, myTasksFilter);
        }

        await context.SaveChangesAsync();
    }
}