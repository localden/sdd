using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Aspire.Testing;
using Xunit;

namespace Taskify.ContractTests;

public class AspireAppHostFixture : IAsyncLifetime
{
    private DistributedApplicationTestingBuilder? _builder;
    private DistributedApplication? _app;
    private readonly Dictionary<string, HttpClient> _httpClients = new();

    public async Task InitializeAsync()
    {
        _builder = DistributedApplicationTestingBuilder.Create<Projects.Taskify_AppHost>();
        
        // Configure test services
        _builder.Services.Configure<LoggerFilterOptions>(options =>
        {
            // Reduce log noise during testing
            options.MinLevel = LogLevel.Warning;
        });
        
        // Build and start the application
        _app = await _builder.BuildAsync();
        await _app.StartAsync();
    }

    public async Task DisposeAsync()
    {
        foreach (var client in _httpClients.Values)
        {
            client.Dispose();
        }
        _httpClients.Clear();

        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }

    public HttpClient CreateHttpClient(string serviceName)
    {
        if (_httpClients.TryGetValue(serviceName, out var existingClient))
        {
            return existingClient;
        }

        if (_app == null)
            throw new InvalidOperationException("Application not initialized");

        var client = _app.CreateHttpClient(serviceName);
        
        // Add default headers for testing
        client.DefaultRequestHeaders.Add("User-Agent", "Taskify-ContractTests/1.0");
        
        // Add authentication token for API calls (mock token for testing)
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

        _httpClients[serviceName] = client;
        return client;
    }

    public string GetEndpoint(string serviceName)
    {
        if (_app == null)
            throw new InvalidOperationException("Application not initialized");

        return _app.GetEndpoint(serviceName).ToString();
    }

    public T GetRequiredService<T>() where T : notnull
    {
        if (_app == null)
            throw new InvalidOperationException("Application not initialized");

        return _app.Services.GetRequiredService<T>();
    }
}