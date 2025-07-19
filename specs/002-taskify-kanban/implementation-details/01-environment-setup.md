# Environment Setup: Enhanced Kanban Board System

**Document**: 002-taskify-kanban/implementation-details/01-environment-setup.md  
**Created**: 2025-07-19  
**Status**: Enhanced development environment configuration  

---

## Overview

This document outlines the enhanced development environment setup required for implementing advanced kanban board functionality as an extension to the existing Taskify application. The setup builds upon the existing .NET Aspire infrastructure while adding mobile testing capabilities and performance monitoring tools.

---

## Prerequisites

### Enhanced System Requirements
- **.NET 9.0 SDK** (existing requirement)
- **Docker Desktop** (existing requirement)
- **PostgreSQL 15+** (existing requirement)
- **Node.js 18+** (new - for mobile testing tools)
- **Browser Developer Tools** (enhanced - for mobile device simulation)

### Enhanced IDE Requirements
- **Visual Studio 2024** or **VS Code** with enhanced extensions:
  - C# Dev Kit (existing)
  - .NET Aspire (existing)
  - **Blazor WASM Debugging** (enhanced)
  - **Browser Preview** (new - for mobile testing)
  - **REST Client** (enhanced - for API testing)

---

## Enhanced Project Structure Setup

### Step 1: Enhance Existing Solution
```bash
# Navigate to existing Taskify solution
cd src/

# Add enhanced NuGet packages for kanban features
dotnet add Taskify.Web package Syncfusion.Blazor.Kanban --version 27.1.48
dotnet add Taskify.Web package Syncfusion.Blazor.Navigations --version 27.1.48
dotnet add Taskify.ApiService package Microsoft.AspNetCore.SignalR.StackExchangeRedis --version 9.0.0

# Add mobile testing and performance packages
dotnet add Taskify.Web package Microsoft.AspNetCore.Components.WebAssembly.DevServer --version 9.0.0
dotnet add Taskify.ApiService package Microsoft.Extensions.Diagnostics.HealthChecks --version 9.0.0
```

### Step 2: Enhanced Aspire Configuration
```csharp
// Enhanced Program.cs in Taskify.AppHost
var builder = DistributedApplication.CreateBuilder(args);

// Enhanced Redis for SignalR backplane
var redis = builder.AddRedis("cache")
    .WithRedisCommander(); // Enhanced with Redis management UI

// Enhanced PostgreSQL with performance monitoring
var postgres = builder.AddPostgreSQL("postgres")
    .WithPgAdmin()
    .WithHealthCheck(); // Enhanced health monitoring

// Enhanced API service with kanban endpoints
var apiService = builder.AddProject<Projects.Taskify_ApiService>("apiservice")
    .WithReference(postgres)
    .WithReference(redis)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithHealthCheck(); // Enhanced monitoring

// Enhanced Web app with mobile debugging
var webApp = builder.AddProject<Projects.Taskify_Web>("webfrontend")
    .WithReference(apiService)
    .WithReference(redis)
    .WithExternalHttpEndpoints() // Enhanced for mobile testing
    .WithHealthCheck();

builder.Build().Run();
```

---

## Enhanced Database Configuration

### Step 1: Enhanced Connection String
```json
// Enhanced appsettings.Development.json in ApiService
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=taskify_dev;Username=postgres;Password=postgres;Include Error Detail=true;Log Parameters=true"
  },
  "SignalR": {
    "Redis": {
      "ConnectionString": "localhost:6379",
      "ChannelPrefix": "taskify_kanban"
    }
  },
  "Kanban": {
    "MaxTasksPerBoard": 1000,
    "MaxConcurrentUsers": 50,
    "RealTimeUpdateIntervalMs": 100
  }
}
```

### Step 2: Enhanced Migration Setup
```bash
# Create enhanced migration for kanban features
cd src/Taskify.ApiService
dotnet ef migrations add KanbanBoardFeatures --output-dir Data/Migrations

# Apply migration to development database
dotnet ef database update
```

---

## Mobile Testing Environment

### Step 1: Browser Configuration for Mobile Testing
```bash
# Install browser testing tools
npm install -g lighthouse
npm install -g @playwright/test

# Configure Chrome for mobile debugging
# Add to launch.json in VS Code:
```

```json
{
  "name": "Launch Chrome Mobile Simulation",
  "request": "launch",
  "type": "chrome",
  "url": "https://localhost:7154",
  "userDataDir": "${workspaceFolder}/.vscode/chrome-mobile",
  "runtimeArgs": [
    "--device-scale-factor=2",
    "--mobile",
    "--user-agent=Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X) AppleWebKit/605.1.15"
  ]
}
```

### Step 2: Device Testing Setup
```csharp
// Enhanced Startup configuration for mobile debugging
public void ConfigureServices(IServiceCollection services)
{
    services.AddBlazorServer(options =>
    {
        // Enhanced for mobile debugging
        options.DetailedErrors = true;
        options.DisconnectedCircuitMaxRetained = 100;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
    });
    
    // Add mobile-specific configuration
    services.Configure<KanbanMobileOptions>(Configuration.GetSection("Kanban:Mobile"));
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        // Enhanced error handling for mobile debugging
        app.UseDeveloperExceptionPage();
        app.UseWebAssemblyDebugging(); // For mobile debugging
    }
    
    // Enhanced HTTPS redirection for mobile testing
    app.UseHttpsRedirection();
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // Enhanced caching for mobile performance
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
        }
    });
}
```

---

## Performance Monitoring Setup

### Step 1: Enhanced Logging Configuration
```json
// Enhanced appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore.SignalR": "Debug",
      "Taskify.ApiService.Controllers.KanbanController": "Debug",
      "Taskify.Web.Components.Kanban": "Debug"
    }
  },
  "OpenTelemetry": {
    "ServiceName": "Taskify.Kanban",
    "JaegerEndpoint": "http://localhost:14268/api/traces"
  }
}
```

### Step 2: Performance Monitoring Implementation
```csharp
// Enhanced Program.cs in ApiService with performance monitoring
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Enhanced telemetry for kanban operations
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddEntityFrameworkCoreInstrumentation()
               .AddRedisInstrumentation() // For SignalR monitoring
               .AddSource("Taskify.Kanban"); // Custom telemetry source
    });

// Enhanced health checks for kanban features
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .AddRedis(builder.Configuration.GetConnectionString("Redis"))
    .AddSignalRHub<KanbanHub>(); // Custom health check for SignalR

var app = builder.Build();

// Enhanced health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

---

## Development Workflow Enhancements

### Step 1: Enhanced Hot Reload Configuration
```json
// Enhanced launchSettings.json for kanban development
{
  "profiles": {
    "Taskify.AppHost": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:15888;http://localhost:15889",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "DOTNET_DASHBOARD_OTLP_ENDPOINT_URL": "https://localhost:18889",
        "KANBAN_DEBUG_MODE": "true"
      }
    }
  }
}
```

### Step 2: Enhanced Testing Scripts
```bash
#!/bin/bash
# Enhanced development script: enhanced-dev-start.sh

echo "Starting enhanced Taskify development environment..."

# Start enhanced containers
docker-compose -f docker-compose.dev.yml up -d postgres redis

# Wait for services
echo "Waiting for database..."
until docker exec taskify_postgres pg_isready; do sleep 1; done

echo "Waiting for Redis..."
until docker exec taskify_redis redis-cli ping; do sleep 1; done

# Apply enhanced migrations
echo "Applying enhanced database migrations..."
dotnet ef database update --project src/Taskify.ApiService

# Start enhanced development servers
echo "Starting enhanced .NET Aspire application..."
dotnet run --project src/Taskify.AppHost

echo "Enhanced development environment ready!"
echo "Web App: https://localhost:7154"
echo "API: https://localhost:7195"
echo "Dashboard: https://localhost:15888"
echo "Mobile Testing: Use Chrome DevTools device simulation"
```

---

## IDE Configuration for Enhanced Development

### Step 1: Enhanced VS Code Settings
```json
// Enhanced .vscode/settings.json
{
  "dotnet.completion.showCompletionItemsFromUnimportedNamespaces": true,
  "omnisharp.enableEditorConfigSupport": true,
  "omnisharp.enableRoslynAnalyzers": true,
  "csharp.semanticHighlighting.enabled": true,
  "blazorwasm.debug.enabled": true,
  "blazorwasm.hotReload.enabled": true,
  "files.exclude": {
    "**/bin": true,
    "**/obj": true,
    "**/.vscode": false
  },
  "search.exclude": {
    "**/node_modules": true,
    "**/bin": true,
    "**/obj": true
  }
}
```

### Step 2: Enhanced Debug Configuration
```json
// Enhanced .vscode/launch.json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch Enhanced Taskify",
      "type": "dotnet",
      "request": "launch",
      "projectPath": "${workspaceFolder}/src/Taskify.AppHost/Taskify.AppHost.csproj",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "KANBAN_DEBUG_MODE": "true"
      }
    },
    {
      "name": "Attach to Kanban API",
      "type": "dotnet",
      "request": "attach",
      "processName": "Taskify.ApiService"
    },
    {
      "name": "Debug Mobile Browser",
      "type": "chrome",
      "request": "launch",
      "url": "https://localhost:7154",
      "webRoot": "${workspaceFolder}/src/Taskify.Web",
      "userDataDir": "${workspaceFolder}/.vscode/chrome-mobile",
      "runtimeArgs": [
        "--device-scale-factor=2",
        "--mobile"
      ]
    }
  ]
}
```

---

## Environment Verification

### Step 1: Enhanced Service Health Checks
```bash
# Enhanced verification script: verify-enhanced-environment.sh
#!/bin/bash

echo "Verifying enhanced Taskify environment..."

# Check .NET version
echo "Checking .NET version..."
dotnet --version

# Check Docker
echo "Checking Docker..."
docker --version

# Check enhanced services
echo "Checking enhanced services..."
curl -f https://localhost:7195/health || echo "API service not ready"
curl -f https://localhost:7154/health || echo "Web service not ready"

# Check enhanced database
echo "Checking enhanced database schema..."
docker exec taskify_postgres psql -U postgres -d taskify_dev -c "\dt" | grep -E "(kanban_boards|board_columns|task_positions)"

# Check Redis for SignalR
echo "Checking Redis for SignalR..."
docker exec taskify_redis redis-cli info replication

echo "Enhanced environment verification complete!"
```

### Step 2: Enhanced Performance Baseline
```bash
# Enhanced performance testing script
#!/bin/bash

echo "Running enhanced performance baseline tests..."

# Test enhanced API endpoints
echo "Testing kanban API performance..."
curl -w "@curl-format.txt" -s -o /dev/null https://localhost:7195/api/kanban/boards

# Test enhanced WebSocket connections
echo "Testing SignalR connection performance..."
# Use custom SignalR test client here

# Test mobile performance
echo "Testing mobile performance..."
lighthouse https://localhost:7154 --only-categories=performance --output=json --output-path=performance-baseline.json

echo "Enhanced performance baseline complete!"
```

---

## Environment Setup Status: COMPLETE ✅

All enhanced environment setup requirements have been documented:
- [x] Enhanced system and IDE requirements specified
- [x] Enhanced project structure and NuGet packages configured
- [x] Enhanced Aspire orchestration setup
- [x] Enhanced database configuration with kanban schema
- [x] Mobile testing environment configured
- [x] Performance monitoring and telemetry setup
- [x] Enhanced development workflow and scripts
- [x] IDE configuration for enhanced debugging
- [x] Environment verification and performance baseline procedures

**Ready to proceed to enhanced API contracts definition**