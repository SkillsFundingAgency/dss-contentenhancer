using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Processor;
using NCS.DSS.ContentEnhancer.Services;

namespace NCS.DSS.ContentEnhancer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();

                    services.AddSingleton<IQueueProcessor, QueueProcessor>();
                    services.AddSingleton<ICosmosDBProvider, CosmosDBProvider>();

                    services.AddSingleton(s =>
                    {
                        var cosmosEndpoint = Environment.GetEnvironmentVariable("Endpoint");
                        var cosmosKey = Environment.GetEnvironmentVariable("Key");

                        return new CosmosClient(cosmosEndpoint, cosmosKey);
                    });

                    // shared services
                    services.AddSingleton<IMessagingService, MessagingService>();
                    services.AddSingleton<ISubscriptionService, SubscriptionService>();

                    services.Configure<LoggerFilterOptions>(options =>
                    {
                        // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
                        // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
                        LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

                        if (toRemove is not null)
                        {
                            options.Rules.Remove(toRemove);
                        }
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}