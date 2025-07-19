var builder = DistributedApplication.CreateBuilder(args);

// Enhanced Redis for SignalR backplane and caching
var redis = builder.AddRedis("cache")
    .WithRedisCommander()
    .WithPersistence();

// Enhanced PostgreSQL with performance monitoring
var postgres = builder.AddPostgreSQL("postgres")
    .WithPgAdmin()
    .WithHealthCheck()
    .WithEnvironment("POSTGRES_DB", "taskify_dev");

// Enhanced API service with kanban endpoints
var apiService = builder.AddProject<Projects.Taskify_ApiService>("apiservice")
    .WithReference(postgres)
    .WithReference(redis)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("Kanban__MaxTasksPerBoard", "1000")
    .WithEnvironment("Kanban__MaxConcurrentUsers", "50")
    .WithEnvironment("Kanban__RealTimeUpdateIntervalMs", "100")
    .WithHealthCheck();

// Enhanced Web app with mobile debugging support
var webApp = builder.AddProject<Projects.Taskify_Web>("webfrontend")
    .WithReference(apiService)
    .WithReference(redis)
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("Kanban__Mobile__EnableTouchOptimization", "true")
    .WithEnvironment("Kanban__Mobile__EnableHapticFeedback", "true")
    .WithHealthCheck();

// Add enhanced monitoring and observability
builder.AddContainer("jaeger", "jaegertracing/all-in-one")
    .WithArgs("--log-level=debug")
    .WithHttpEndpoint(port: 16686, targetPort: 16686, name: "jaeger-ui")
    .WithOtlpEndpoint(port: 4317, targetPort: 4317)
    .WithOtlpEndpoint(port: 4318, targetPort: 4318, scheme: "http");

builder.Build().Run();