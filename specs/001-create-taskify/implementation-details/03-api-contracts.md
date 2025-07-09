# API Contracts: REST API Specifications

## Projects API

### GET /api/projects
Returns all projects with task counts per status column.

**Response:**
```json
{
  "projects": [
    {
      "id": 1,
      "title": "Mobile App Redesign",
      "description": "Complete redesign of mobile application",
      "taskCounts": {
        "toDo": 6,
        "inProgress": 4,
        "inReview": 3,
        "done": 8
      }
    }
  ]
}
```

### GET /api/projects/{id}
Returns specific project with all tasks organized by status.

**Response:**
```json
{
  "id": 1,
  "title": "Mobile App Redesign",
  "description": "Complete redesign of mobile application",
  "tasks": {
    "toDo": [
      {
        "id": 1,
        "title": "User research survey",
        "description": "Conduct user research survey",
        "assignedUser": {
          "id": 1,
          "name": "Sarah Chen",
          "role": "Product Manager"
        },
        "created": "2025-01-01T10:00:00Z",
        "lastModified": "2025-01-01T10:00:00Z"
      }
    ],
    "inProgress": [],
    "inReview": [],
    "done": []
  }
}
```

## Tasks API

### GET /api/tasks/{id}
Returns specific task with comments.

**Response:**
```json
{
  "id": 1,
  "title": "User research survey",
  "description": "Conduct user research survey",
  "status": "ToDo",
  "projectId": 1,
  "assignedUser": {
    "id": 1,
    "name": "Sarah Chen",
    "role": "Product Manager"
  },
  "comments": [
    {
      "id": 1,
      "content": "Starting research phase",
      "author": {
        "id": 1,
        "name": "Sarah Chen"
      },
      "created": "2025-01-01T10:00:00Z",
      "lastModified": null
    }
  ],
  "created": "2025-01-01T10:00:00Z",
  "lastModified": "2025-01-01T10:00:00Z"
}
```

### PUT /api/tasks/{id}
Updates task status and/or assignment.

**Request:**
```json
{
  "status": "InProgress",
  "assignedUserId": 2
}
```

**Response:**
```json
{
  "id": 1,
  "title": "User research survey",
  "status": "InProgress",
  "assignedUser": {
    "id": 2,
    "name": "Alex Rodriguez",
    "role": "Senior Engineer"
  },
  "lastModified": "2025-01-01T11:00:00Z"
}
```

### POST /api/tasks/{id}/comments
Adds comment to a task.

**Request:**
```json
{
  "content": "Research phase completed successfully",
  "authorId": 1
}
```

**Response:**
```json
{
  "id": 5,
  "content": "Research phase completed successfully",
  "author": {
    "id": 1,
    "name": "Sarah Chen"
  },
  "created": "2025-01-01T12:00:00Z",
  "lastModified": null
}
```

### PUT /api/tasks/{taskId}/comments/{commentId}
Updates existing comment (only by author).

**Request:**
```json
{
  "content": "Updated comment content",
  "authorId": 1
}
```

### DELETE /api/tasks/{taskId}/comments/{commentId}
Deletes comment (only by author).

**Query Parameters:**
- `authorId`: ID of user attempting deletion

## Notifications API

### WebSocket /notifications
Real-time notifications for task updates.

**Connection:**
```
ws://localhost:5000/notifications
```

**Message Types:**

#### Task Status Changed
```json
{
  "type": "TaskStatusChanged",
  "taskId": 1,
  "newStatus": "InProgress",
  "projectId": 1,
  "timestamp": "2025-01-01T11:00:00Z"
}
```

#### Task Assignment Changed
```json
{
  "type": "TaskAssignmentChanged",
  "taskId": 1,
  "newAssignedUser": {
    "id": 2,
    "name": "Alex Rodriguez"
  },
  "projectId": 1,
  "timestamp": "2025-01-01T11:00:00Z"
}
```

#### Comment Added
```json
{
  "type": "CommentAdded",
  "taskId": 1,
  "comment": {
    "id": 5,
    "content": "New comment",
    "author": {
      "id": 1,
      "name": "Sarah Chen"
    },
    "created": "2025-01-01T12:00:00Z"
  },
  "projectId": 1,
  "timestamp": "2025-01-01T12:00:00Z"
}
```

## Error Responses

### 400 Bad Request
```json
{
  "error": "Bad Request",
  "message": "Invalid task status",
  "details": "Status must be one of: ToDo, InProgress, InReview, Done"
}
```

### 404 Not Found
```json
{
  "error": "Not Found",
  "message": "Task not found",
  "details": "Task with ID 999 does not exist"
}
```

### 403 Forbidden
```json
{
  "error": "Forbidden",
  "message": "Cannot modify comment",
  "details": "Only comment author can edit or delete comments"
}
```

## API Standards

- All timestamps in ISO 8601 format (UTC)
- Consistent error response format
- RESTful resource naming
- HTTP status codes follow REST conventions
- JSON content type for all requests/responses