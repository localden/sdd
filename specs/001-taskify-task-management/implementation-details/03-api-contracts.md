# API Contracts: OpenAPI Specifications and Controller Design

**Created**: 2025-07-19  
**Status**: Ready for Implementation  

---

## API Architecture Overview

**Three REST APIs**:
1. **Projects API** - CRUD operations for projects and team management
2. **Tasks API** - CRUD operations, status updates, assignments, kanban operations
3. **Notifications API** - Send notifications, get user notifications, mark as read

**Authentication**: JWT Bearer tokens for all endpoints
**Error Format**: RFC 7807 Problem Details standard
**Response Format**: JSON with consistent patterns

## Projects API Specification

### Base Path: `/api/projects`

#### GET /api/projects
Get all projects for authenticated user
```json
{
  "summary": "Get user projects",
  "security": [{"Bearer": []}],
  "responses": {
    "200": {
      "description": "List of projects",
      "content": {
        "application/json": {
          "schema": {
            "type": "array",
            "items": {"$ref": "#/components/schemas/ProjectDto"}
          }
        }
      }
    }
  }
}
```

#### POST /api/projects
Create new project
```json
{
  "summary": "Create project",
  "security": [{"Bearer": []}],
  "requestBody": {
    "content": {
      "application/json": {
        "schema": {"$ref": "#/components/schemas/CreateProjectRequest"}
      }
    }
  },
  "responses": {
    "201": {
      "description": "Project created",
      "content": {
        "application/json": {
          "schema": {"$ref": "#/components/schemas/ProjectDto"}
        }
      }
    },
    "400": {"$ref": "#/components/responses/BadRequest"},
    "401": {"$ref": "#/components/responses/Unauthorized"}
  }
}
```

#### GET /api/projects/{projectId}
Get project by ID
```json
{
  "summary": "Get project details",
  "security": [{"Bearer": []}],
  "parameters": [
    {
      "name": "projectId",
      "in": "path",
      "required": true,
      "schema": {"type": "string"}
    }
  ],
  "responses": {
    "200": {
      "description": "Project details",
      "content": {
        "application/json": {
          "schema": {"$ref": "#/components/schemas/ProjectDetailDto"}
        }
      }
    },
    "404": {"$ref": "#/components/responses/NotFound"}
  }
}
```

#### PUT /api/projects/{projectId}
Update project
```json
{
  "summary": "Update project",
  "security": [{"Bearer": []}],
  "parameters": [
    {
      "name": "projectId",
      "in": "path",
      "required": true,
      "schema": {"type": "string"}
    }
  ],
  "requestBody": {
    "content": {
      "application/json": {
        "schema": {"$ref": "#/components/schemas/UpdateProjectRequest"}
      }
    }
  },
  "responses": {
    "200": {
      "description": "Project updated",
      "content": {
        "application/json": {
          "schema": {"$ref": "#/components/schemas/ProjectDto"}
        }
      }
    }
  }
}
```

#### POST /api/projects/{projectId}/members
Add team member to project
```json
{
  "summary": "Add project member",
  "security": [{"Bearer": []}],
  "parameters": [
    {
      "name": "projectId",
      "in": "path",
      "required": true,
      "schema": {"type": "string"}
    }
  ],
  "requestBody": {
    "content": {
      "application/json": {
        "schema": {"$ref": "#/components/schemas/AddMemberRequest"}
      }
    }
  },
  "responses": {
    "200": {"description": "Member added successfully"}
  }
}
```

## Tasks API Specification

### Base Path: `/api/tasks`

#### GET /api/tasks/project/{projectId}
Get all tasks for a project (kanban board data)
```json
{
  "summary": "Get project tasks",
  "security": [{"Bearer": []}],
  "parameters": [
    {
      "name": "projectId",
      "in": "path",
      "required": true,
      "schema": {"type": "string"}
    }
  ],
  "responses": {
    "200": {
      "description": "Project tasks grouped by status",
      "content": {
        "application/json": {
          "schema": {"$ref": "#/components/schemas/KanbanBoardDto"}
        }
      }
    }
  }
}
```

#### POST /api/tasks
Create new task
```json
{
  "summary": "Create task",
  "security": [{"Bearer": []}],
  "requestBody": {
    "content": {
      "application/json": {
        "schema": {"$ref": "#/components/schemas/CreateTaskRequest"}
      }
    }
  },
  "responses": {
    "201": {
      "description": "Task created",
      "content": {
        "application/json": {
          "schema": {"$ref": "#/components/schemas/TaskDto"}
        }
      }
    }
  }
}
```

#### PUT /api/tasks/{taskId}/move
Move task (drag-and-drop operation)
```json
{
  "summary": "Move task to different status/position",
  "security": [{"Bearer": []}],
  "parameters": [
    {
      "name": "taskId",
      "in": "path",
      "required": true,
      "schema": {"type": "string"}
    }
  ],
  "requestBody": {
    "content": {
      "application/json": {
        "schema": {"$ref": "#/components/schemas/MoveTaskRequest"}
      }
    }
  },
  "responses": {
    "200": {
      "description": "Task moved successfully",
      "content": {
        "application/json": {
          "schema": {"$ref": "#/components/schemas/TaskDto"}
        }
      }
    }
  }
}
```

#### PUT /api/tasks/{taskId}/assign
Assign task to user
```json
{
  "summary": "Assign task to team member",
  "security": [{"Bearer": []}],
  "parameters": [
    {
      "name": "taskId",
      "in": "path",
      "required": true,
      "schema": {"type": "string"}
    }
  ],
  "requestBody": {
    "content": {
      "application/json": {
        "schema": {"$ref": "#/components/schemas/AssignTaskRequest"}
      }
    }
  },
  "responses": {
    "200": {
      "description": "Task assigned successfully"
    }
  }
}
```

## Notifications API Specification

### Base Path: `/api/notifications`

#### GET /api/notifications
Get user notifications
```json
{
  "summary": "Get user notifications",
  "security": [{"Bearer": []}],
  "parameters": [
    {
      "name": "page",
      "in": "query",
      "schema": {"type": "integer", "default": 1}
    },
    {
      "name": "pageSize",
      "in": "query",
      "schema": {"type": "integer", "default": 20}
    },
    {
      "name": "unreadOnly",
      "in": "query",
      "schema": {"type": "boolean", "default": false}
    }
  ],
  "responses": {
    "200": {
      "description": "User notifications",
      "content": {
        "application/json": {
          "schema": {"$ref": "#/components/schemas/NotificationListDto"}
        }
      }
    }
  }
}
```

#### PUT /api/notifications/{notificationId}/read
Mark notification as read
```json
{
  "summary": "Mark notification as read",
  "security": [{"Bearer": []}],
  "parameters": [
    {
      "name": "notificationId",
      "in": "path",
      "required": true,
      "schema": {"type": "string"}
    }
  ],
  "responses": {
    "200": {"description": "Notification marked as read"}
  }
}
```

#### PUT /api/notifications/mark-all-read
Mark all notifications as read
```json
{
  "summary": "Mark all notifications as read",
  "security": [{"Bearer": []}],
  "responses": {
    "200": {"description": "All notifications marked as read"}
  }
}
```

## Data Transfer Objects (DTOs)

### ProjectDto
```json
{
  "type": "object",
  "properties": {
    "id": {"type": "string"},
    "name": {"type": "string"},
    "description": {"type": "string", "nullable": true},
    "ownerId": {"type": "string"},
    "ownerName": {"type": "string"},
    "memberCount": {"type": "integer"},
    "taskCount": {"type": "integer"},
    "createdAt": {"type": "string", "format": "date-time"},
    "updatedAt": {"type": "string", "format": "date-time"}
  },
  "required": ["id", "name", "ownerId", "ownerName", "memberCount", "taskCount", "createdAt", "updatedAt"]
}
```

### TaskDto
```json
{
  "type": "object",
  "properties": {
    "id": {"type": "string"},
    "title": {"type": "string"},
    "description": {"type": "string", "nullable": true},
    "status": {"type": "string", "enum": ["ToDo", "InProgress", "Done"]},
    "priority": {"type": "string", "enum": ["Low", "Medium", "High", "Critical"]},
    "position": {"type": "integer"},
    "projectId": {"type": "string"},
    "createdById": {"type": "string"},
    "createdByName": {"type": "string"},
    "assignedToId": {"type": "string", "nullable": true},
    "assignedToName": {"type": "string", "nullable": true},
    "dueDate": {"type": "string", "format": "date-time", "nullable": true},
    "createdAt": {"type": "string", "format": "date-time"},
    "updatedAt": {"type": "string", "format": "date-time"}
  }
}
```

### KanbanBoardDto
```json
{
  "type": "object",
  "properties": {
    "projectId": {"type": "string"},
    "projectName": {"type": "string"},
    "columns": {
      "type": "object",
      "properties": {
        "toDo": {
          "type": "array",
          "items": {"$ref": "#/components/schemas/TaskDto"}
        },
        "inProgress": {
          "type": "array",
          "items": {"$ref": "#/components/schemas/TaskDto"}
        },
        "done": {
          "type": "array",
          "items": {"$ref": "#/components/schemas/TaskDto"}
        }
      }
    }
  }
}
```

### Request DTOs

#### CreateTaskRequest
```json
{
  "type": "object",
  "properties": {
    "title": {"type": "string", "maxLength": 200},
    "description": {"type": "string", "maxLength": 2000, "nullable": true},
    "priority": {"type": "string", "enum": ["Low", "Medium", "High", "Critical"]},
    "projectId": {"type": "string"},
    "assignedToId": {"type": "string", "nullable": true},
    "dueDate": {"type": "string", "format": "date-time", "nullable": true}
  },
  "required": ["title", "projectId"]
}
```

#### MoveTaskRequest
```json
{
  "type": "object",
  "properties": {
    "fromStatus": {"type": "string", "enum": ["ToDo", "InProgress", "Done"]},
    "toStatus": {"type": "string", "enum": ["ToDo", "InProgress", "Done"]},
    "position": {"type": "integer", "minimum": 0}
  },
  "required": ["fromStatus", "toStatus", "position"]
}
```

#### AssignTaskRequest
```json
{
  "type": "object",
  "properties": {
    "assignedToId": {"type": "string", "nullable": true}
  }
}
```

## Controller Implementation Guidelines

### Base Controller Structure
```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly TaskifyDbContext _context;
    private readonly IHubContext<TaskNotificationHub> _hubContext;
    private readonly IEmailService _emailService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(
        TaskifyDbContext context,
        IHubContext<TaskNotificationHub> hubContext,
        IEmailService emailService,
        ILogger<TasksController> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _emailService = emailService;
        _logger = logger;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException();
    }
}
```

### Error Handling Pattern
```csharp
[HttpGet("project/{projectId}")]
public async Task<ActionResult<KanbanBoardDto>> GetProjectTasks(string projectId)
{
    try
    {
        var currentUserId = GetCurrentUserId();
        
        // Verify user has access to project
        var hasAccess = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUserId);
            
        if (!hasAccess)
        {
            return Forbid();
        }

        var tasks = await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .OrderBy(t => t.Status)
            .ThenBy(t => t.Position)
            .ToListAsync();

        var kanbanBoard = MapToKanbanBoard(projectId, tasks);
        return Ok(kanbanBoard);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to get tasks for project {ProjectId}", projectId);
        return StatusCode(500, new ProblemDetails
        {
            Title = "Internal Server Error",
            Detail = "An error occurred while retrieving tasks"
        });
    }
}
```

### SignalR Integration Pattern
```csharp
[HttpPut("{taskId}/move")]
public async Task<ActionResult<TaskDto>> MoveTask(string taskId, MoveTaskRequest request)
{
    try
    {
        var currentUserId = GetCurrentUserId();
        
        var task = await _context.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId);
            
        if (task == null)
        {
            return NotFound();
        }

        // Update task status and position
        task.Status = Enum.Parse<TaskStatus>(request.ToStatus);
        task.Position = request.Position;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Broadcast update via SignalR
        await _hubContext.Clients.Group($"project-{task.ProjectId}")
            .SendAsync("TaskMoved", new
            {
                TaskId = taskId,
                FromStatus = request.FromStatus,
                ToStatus = request.ToStatus,
                Position = request.Position,
                UpdatedBy = currentUserId
            });

        var taskDto = MapToTaskDto(task);
        return Ok(taskDto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to move task {TaskId}", taskId);
        return StatusCode(500, new ProblemDetails
        {
            Title = "Task Move Failed",
            Detail = "An error occurred while moving the task"
        });
    }
}
```

## Authentication Configuration

### JWT Bearer Setup in Program.cs
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
```

## Error Response Standards

### Problem Details Format
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The request body contains invalid data",
  "instance": "/api/tasks",
  "errors": {
    "title": ["The title field is required"],
    "projectId": ["Invalid project ID format"]
  }
}
```

### Common Response Components
```json
{
  "components": {
    "responses": {
      "BadRequest": {
        "description": "Bad Request",
        "content": {
          "application/json": {
            "schema": {"$ref": "#/components/schemas/ProblemDetails"}
          }
        }
      },
      "Unauthorized": {
        "description": "Unauthorized",
        "content": {
          "application/json": {
            "schema": {"$ref": "#/components/schemas/ProblemDetails"}
          }
        }
      },
      "NotFound": {
        "description": "Not Found",
        "content": {
          "application/json": {
            "schema": {"$ref": "#/components/schemas/ProblemDetails"}
          }
        }
      }
    }
  }
}
```

## Next Steps

1. **Implementation**: Use these contracts to implement controllers in Phase 1
2. **Testing**: Reference **06-contract-tests.md** for test implementation
3. **Integration**: See **05-integrations.md** for SignalR hub integration
4. **Validation**: Follow **manual-testing.md** for API endpoint testing

## OpenAPI Documentation

The complete OpenAPI 3.0 specification will be generated automatically via Swashbuckle and available at `/swagger` endpoint during development.