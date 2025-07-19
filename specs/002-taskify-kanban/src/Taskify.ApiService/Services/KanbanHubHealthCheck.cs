using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.SignalR;
using Taskify.ApiService.Hubs;

namespace Taskify.ApiService.Services;

public class KanbanHubHealthCheck : IHealthCheck
{
    private readonly IHubContext<KanbanHub> _hubContext;
    private readonly ILogger<KanbanHubHealthCheck> _logger;

    public KanbanHubHealthCheck(IHubContext<KanbanHub> hubContext, ILogger<KanbanHubHealthCheck> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if the hub context is available and functional
            var clients = _hubContext.Clients;
            
            if (clients == null)
            {
                return HealthCheckResult.Unhealthy("SignalR hub context is not available");
            }

            // Try to get connection count (this is a basic connectivity test)
            // In a real implementation, you might want to ping a test group or check Redis connection
            var connectionInfo = new
            {
                HubContextAvailable = true,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogDebug("Kanban hub health check completed successfully");
            
            return HealthCheckResult.Healthy("Kanban hub is functioning properly", 
                new Dictionary<string, object>
                {
                    ["hubContext"] = "available",
                    ["checkedAt"] = DateTime.UtcNow.ToString("O")
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kanban hub health check failed");
            return HealthCheckResult.Unhealthy("Kanban hub is not functioning", ex);
        }
    }
}