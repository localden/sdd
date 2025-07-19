using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Taskify.ApiService.Data;
using Taskify.ApiService.Hubs;
using Taskify.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Enhanced logging configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();

// Add Aspire service defaults with enhanced monitoring
builder.AddServiceDefaults();

// Enhanced database configuration
builder.AddNpgsqlDbContext<TaskifyDbContext>("postgres", configureDbContextOptions: options =>
{
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

// Enhanced Redis configuration for SignalR backplane
builder.AddStackExchangeRedisClient("cache");

// Enhanced telemetry for kanban operations
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddEntityFrameworkCoreInstrumentation()
               .AddRedisInstrumentation()
               .AddSource("Taskify.Kanban");
    });

// Core services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Enhanced SignalR with Redis backplane
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
})
.AddStackExchangeRedis(connectionString => 
{
    var redis = builder.Configuration.GetConnectionString("cache") ?? "localhost:6379";
    return redis;
}, options =>
{
    options.Configuration.ChannelPrefix = "taskify_kanban";
});

// Enhanced authentication and authorization
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = false
        };
        
        // Configure SignalR authentication
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Enhanced API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Taskify Enhanced Kanban API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Enhanced health checks for kanban features
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("postgres")!)
    .AddStackExchangeRedis(builder.Configuration.GetConnectionString("cache")!)
    .AddCheck<KanbanHubHealthCheck>("kanban_hub");

// Enhanced application services
builder.Services.AddScoped<IKanbanService, KanbanService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<KanbanHubHealthCheck>();

// CORS configuration for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7154", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Taskify Enhanced Kanban API V1");
        c.RoutePrefix = "swagger";
    });
    app.UseCors("AllowWebFrontend");
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Enhanced SignalR hubs for real-time kanban operations
app.MapHub<KanbanHub>("/hubs/kanban");

// Map Aspire service defaults
app.MapDefaultEndpoints();

// Enhanced database initialization
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskifyDbContext>();
    await context.Database.EnsureCreatedAsync();
    
    if (app.Environment.IsDevelopment())
    {
        await TaskifyDataSeeder.SeedAsync(context);
    }
}

app.Run();