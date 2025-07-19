using Microsoft.AspNetCore.SignalR.Client;
using Syncfusion.Blazor;
using Serilog;
using Taskify.Web.Services;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Enhanced logging configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add Aspire service defaults
builder.AddServiceDefaults();

// Enhanced Redis for session management and SignalR
builder.AddStackExchangeRedisClient("cache");

// Enhanced Blazor Server configuration
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    // Enhanced for mobile debugging and kanban operations
    options.DetailedErrors = builder.Environment.IsDevelopment();
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(2);
})
.AddCircuitOptions(options =>
{
    // Enhanced for real-time kanban operations
    options.DetailedErrors = builder.Environment.IsDevelopment();
});

// Enhanced Syncfusion configuration
builder.Services.AddSyncfusionBlazor();

// Enhanced HTTP client for API communication
builder.Services.AddHttpClient<IApiClient, ApiClient>("TaskifyApi", client =>
{
    var apiBaseUrl = builder.Configuration["Services:ApiService:Https:0"] ?? "https://localhost:7195";
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", "Taskify-Web/1.0");
})
.AddStandardResilienceHandler();

// Enhanced SignalR client for real-time kanban updates
builder.Services.AddSingleton<IKanbanSignalRService>(provider =>
{
    var apiBaseUrl = builder.Configuration["Services:ApiService:Https:0"] ?? "https://localhost:7195";
    var hubUrl = $"{apiBaseUrl}/hubs/kanban";
    
    var connection = new HubConnectionBuilder()
        .WithUrl(hubUrl)
        .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
        .Build();
    
    return new KanbanSignalRService(connection, provider.GetRequiredService<ILogger<KanbanSignalRService>>());
});

// Enhanced telemetry for kanban operations
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddSource("Taskify.Web.Kanban");
    });

// Enhanced authentication
builder.Services.AddAuthentication("Cookie")
    .AddCookie("Cookie", options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Enhanced application services
builder.Services.AddScoped<IKanbanBoardService, KanbanBoardService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Enhanced health checks
builder.Services.AddHealthChecks()
    .AddStackExchangeRedis(builder.Configuration.GetConnectionString("cache")!)
    .AddUrlGroup(new Uri($"{builder.Configuration["Services:ApiService:Https:0"]}/health"), "api_health");

var app = builder.Build();

// Configure the HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapHealthChecks("/health");

// Map Aspire service defaults
app.MapDefaultEndpoints();

app.Run();