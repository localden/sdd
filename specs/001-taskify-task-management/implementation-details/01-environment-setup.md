# Environment Setup: Taskify Development Environment

**Created**: 2025-07-19  
**Status**: Ready for Implementation  

---

## Prerequisites

### Required Software
- **.NET 9.0 SDK** (latest version)
- **Docker Desktop** (for PostgreSQL and container management)
- **Visual Studio 2022 17.8+** or **VS Code** with C# Dev Kit
- **Git** for version control

### Installation Commands
```bash
# Verify .NET 9.0 installation
dotnet --version

# Install Aspire workload (optional - we use NuGet packages)
# dotnet workload install aspire

# Verify Docker is running
docker --version
docker ps
```

## Project Structure Setup

### 1. Create Solution Structure
```bash
# Create solution directory
mkdir taskify-aspire
cd taskify-aspire

# Create solution file
dotnet new sln -n Taskify

# Create projects
dotnet new aspire-apphost -n Taskify.AppHost
dotnet new webapi -n Taskify.ApiService
dotnet new blazorserver -n Taskify.Web

# Add projects to solution
dotnet sln add Taskify.AppHost/Taskify.AppHost.csproj
dotnet sln add Taskify.ApiService/Taskify.ApiService.csproj
dotnet sln add Taskify.Web/Taskify.Web.csproj
```

### 2. Add NuGet Package References

**Taskify.AppHost.csproj:**
```xml
<PackageReference Include="Aspire.Hosting.AppHost" Version="9.3.1" />
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.3.1" />
```

**Taskify.ApiService.csproj:**
```xml
<PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.3.1" />
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.OpenTelemetry" Version="4.1.0" />
```

**Taskify.Web.csproj:**
```xml
<PackageReference Include="Syncfusion.Blazor.Kanban" Version="29.2.5" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="9.0.0" />
```

### 3. Configure Syncfusion License
Add to Taskify.Web/Program.cs before builder.Build():
```csharp
// Register Syncfusion license
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("your-license-key");
```

## Development Database Setup

### 1. PostgreSQL via Docker
The AppHost will automatically manage PostgreSQL containers, but for manual setup:

```bash
# Run PostgreSQL container
docker run --name taskify-postgres \
  -e POSTGRES_PASSWORD=dev_password \
  -e POSTGRES_DB=taskifydb \
  -p 5432:5432 \
  -d postgres:15

# Run pgAdmin (optional)
docker run --name taskify-pgadmin \
  -e PGADMIN_DEFAULT_EMAIL=admin@taskify.local \
  -e PGADMIN_DEFAULT_PASSWORD=admin \
  -p 8080:80 \
  -d dpage/pgadmin4
```

### 2. Entity Framework Setup
```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Create initial migration (run from ApiService directory)
cd Taskify.ApiService
dotnet ef migrations add InitialCreate
```

## SMTP Development Setup

### Option 1: MailDev (Recommended for Development)
```bash
# Run MailDev container for email testing
docker run --name taskify-maildev \
  -p 1080:1080 \
  -p 1025:1025 \
  -d maildev/maildev
```
- Web interface: http://localhost:1080
- SMTP server: localhost:1025

### Option 2: Ethereal Email (Online Testing)
- Create account at https://ethereal.email/
- Use provided SMTP credentials in development

## IDE Configuration

### Visual Studio 2022
1. Set multiple startup projects:
   - Right-click solution → Properties
   - Set Taskify.AppHost as startup project
2. Install extensions:
   - Docker support
   - Git integration

### VS Code
Required extensions:
- C# Dev Kit
- Docker
- REST Client (for API testing)

Create `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch Taskify.AppHost",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/Taskify.AppHost/bin/Debug/net9.0/Taskify.AppHost.dll",
      "cwd": "${workspaceFolder}/Taskify.AppHost",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

## Configuration Files

### appsettings.Development.json (ApiService)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "EmailSettings": {
    "Host": "localhost",
    "Port": 1025,
    "EnableSsl": false,
    "Username": "",
    "Password": "",
    "FromEmail": "[email protected]",
    "FromName": "Taskify Development"
  },
  "Jwt": {
    "Key": "your-development-secret-key-must-be-at-least-256-bits-long",
    "Issuer": "https://taskify.local",
    "Audience": "https://taskify.local"
  }
}
```

### appsettings.Development.json (Web)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Jwt": {
    "Key": "your-development-secret-key-must-be-at-least-256-bits-long",
    "Issuer": "https://taskify.local",
    "Audience": "https://taskify.local"
  }
}
```

## Verification Steps

### 1. Build Verification
```bash
# Restore and build solution
dotnet restore
dotnet build

# Run AppHost (starts all services)
cd Taskify.AppHost
dotnet run
```

### 2. Service Health Checks
After running AppHost, verify:
- Aspire Dashboard: http://localhost:15888
- API Service: https://localhost:7154/swagger
- Web Application: https://localhost:7001
- PostgreSQL: Connection via pgAdmin or database tool
- MailDev: http://localhost:1080

### 3. Development Workflow
1. Start AppHost (starts all services)
2. Make changes to ApiService or Web
3. Services auto-reload on file changes
4. View logs in Aspire Dashboard
5. Test emails in MailDev interface

## Common Issues and Solutions

### Issue: Docker Permission Errors
**Solution**: Ensure Docker Desktop is running and user has permissions

### Issue: Port Conflicts
**Solution**: Check for processes using ports 5432, 1025, 1080, 7154, 7001

### Issue: EF Core Migrations
**Solution**: Ensure PostgreSQL is running before running migrations

### Issue: Syncfusion License
**Solution**: Get trial license from Syncfusion website or use community license

### Issue: SignalR Connection Failures
**Solution**: Check CORS configuration and ensure services are running

## Next Steps

After environment setup:
1. Proceed to **02-data-model.md** for database schema design
2. Follow **03-api-contracts.md** for API specification
3. Use **manual-testing.md** for end-to-end validation

## Troubleshooting Resources

- .NET Aspire Documentation: https://learn.microsoft.com/en-us/dotnet/aspire/
- Syncfusion Blazor Docs: https://blazor.syncfusion.com/documentation/
- SignalR Troubleshooting: https://docs.microsoft.com/en-us/aspnet/core/signalr/troubleshoot