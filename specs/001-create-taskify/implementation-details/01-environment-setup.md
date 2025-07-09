# Environment Setup: Development Environment Configuration

## Prerequisites

### Required Software
- .NET 8.0 SDK or later
- Docker Desktop (for PostgreSQL)
- Visual Studio 2022 or VS Code with C# extension
- Git (for version control)

### Optional Tools
- Azure Data Studio or pgAdmin (for database management)
- Postman or similar (for API testing)
- Browser developer tools (for debugging)

## .NET Aspire Installation

### Install .NET Aspire Workload
```bash
dotnet workload install aspire
```

### Verify Installation
```bash
dotnet workload list
```
Should show "aspire" in the installed workloads list.

### Update to Latest Version
```bash
dotnet workload update
```

## Project Setup

### Create Project Structure
```bash
mkdir taskify
cd taskify
dotnet new aspire-starter -n Taskify
```

### Project Template Structure
```
Taskify/
├── Taskify.AppHost/          # Aspire orchestration
├── Taskify.ServiceDefaults/  # Shared service configuration
├── Taskify.Api/             # REST API project
├── Taskify.Web/             # Blazor Server project
└── Taskify.Tests/           # Test project
```

### Add Required Packages
```bash
# API Project
cd Taskify.Api
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Swashbuckle.AspNetCore

# Web Project
cd ../Taskify.Web
dotnet add package Microsoft.AspNetCore.SignalR.Client
dotnet add package Microsoft.EntityFrameworkCore.Design

# Test Project
cd ../Taskify.Tests
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.SignalR.Client
```

## Database Setup

### PostgreSQL Container Configuration
Create `docker-compose.yml` in project root:
```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    container_name: taskify-postgres
    environment:
      POSTGRES_DB: taskify
      POSTGRES_USER: taskify_user
      POSTGRES_PASSWORD: taskify_password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: unless-stopped

volumes:
  postgres_data:
```

### Start Database
```bash
docker-compose up -d postgres
```

### Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=taskify;Username=taskify_user;Password=taskify_password"
  }
}
```

## .NET Aspire Configuration

### AppHost Configuration
```csharp
// Taskify.AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDatabase("taskify");

var api = builder.AddProject<Projects.Taskify_Api>("taskify-api")
    .WithReference(postgres);

var web = builder.AddProject<Projects.Taskify_Web>("taskify-web")
    .WithReference(api);

builder.Build().Run();
```

### Service Defaults Configuration
```csharp
// Taskify.ServiceDefaults/Extensions.cs
public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });
        return builder;
    }
}
```

## Development Tools Setup

### Visual Studio Configuration
1. **Install Extensions:**
   - C# Dev Kit (if using VS Code)
   - .NET Aspire extension
   - Docker extension

2. **Configure Debugging:**
   - Set multiple startup projects
   - Configure Docker debugging
   - Set up hot reload

### Git Configuration
```bash
# Initialize repository
git init
git add .
git commit -m "Initial project setup"

# Create .gitignore
echo "bin/
obj/
.vs/
*.user
*.suo
appsettings.Development.json
docker-compose.override.yml" > .gitignore
```

## Environment Variables

### Development Environment
Create `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=taskify;Username=taskify_user;Password=taskify_password"
  },
  "AllowedHosts": "*"
}
```

### Production Environment
Create `appsettings.Production.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "${DATABASE_CONNECTION_STRING}"
  },
  "AllowedHosts": "*"
}
```

## Build and Run

### Build All Projects
```bash
dotnet build
```

### Run with .NET Aspire
```bash
cd Taskify.AppHost
dotnet run
```

### Run Individual Services
```bash
# API Service
cd Taskify.Api
dotnet run

# Web Service
cd Taskify.Web
dotnet run
```

### Run Tests
```bash
cd Taskify.Tests
dotnet test
```

## Database Migrations

### Create Initial Migration
```bash
cd Taskify.Api
dotnet ef migrations add InitialCreate
```

### Update Database
```bash
dotnet ef database update
```

### Seed Development Data
```bash
dotnet run -- --seed-data
```

## Verification

### Health Checks
- API Health: https://localhost:7001/health
- Web Health: https://localhost:7000/health
- Database Connection: Check via health endpoint

### Service Discovery
- Aspire Dashboard: https://localhost:15888
- Service Map: View all services and their health

### API Documentation
- Swagger UI: https://localhost:7001/swagger
- OpenAPI Spec: https://localhost:7001/swagger/v1/swagger.json

## Troubleshooting

### Common Issues

#### Port Conflicts
```bash
# Check port usage
netstat -ano | findstr :7000
netstat -ano | findstr :7001
netstat -ano | findstr :5432
```

#### Database Connection Issues
```bash
# Test PostgreSQL connection
docker exec -it taskify-postgres psql -U taskify_user -d taskify -c "SELECT 1;"
```

#### .NET Aspire Issues
```bash
# Clear workload cache
dotnet workload clean
dotnet workload install aspire --skip-manifest-update
```

### Performance Optimization

#### Database Indexing
```sql
-- Create indexes for better performance
CREATE INDEX IX_Tasks_ProjectId_Status ON Tasks(ProjectId, Status);
CREATE INDEX IX_Tasks_AssignedUserId ON Tasks(AssignedUserId);
CREATE INDEX IX_Comments_TaskId ON Comments(TaskId);
```

#### Connection Pooling
```csharp
// Configure connection pooling
services.AddDbContext<TaskifyDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    }));
```

## Security Configuration

### HTTPS Configuration
```csharp
// Configure HTTPS redirection
app.UseHttpsRedirection();
app.UseHsts();
```

### CORS Configuration
```csharp
// Configure CORS for API
services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### Input Validation
```csharp
// Configure model validation
services.AddControllers(options =>
{
    options.ModelValidatorProviders.Clear();
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        _ => "Value cannot be null");
});
```

## Monitoring and Logging

### Application Insights
```csharp
// Configure Application Insights
services.AddApplicationInsightsTelemetry();
```

### Structured Logging
```csharp
// Configure Serilog
services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces));
```

## Next Steps

1. **Complete Environment Setup**: Verify all components are working
2. **Run Initial Tests**: Execute basic health checks
3. **Begin Implementation**: Start with contract definitions
4. **Continuous Integration**: Set up CI/CD pipeline
5. **Monitoring Setup**: Configure observability tools