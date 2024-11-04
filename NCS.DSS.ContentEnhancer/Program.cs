using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.ContentEnhancer.Cosmos.Client;
using NCS.DSS.ContentEnhancer.Cosmos.Helper;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Processor;
using NCS.DSS.ContentEnhancer.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IQueueProcessor, QueueProcessor>();
        services.AddSingleton<IDocumentDBHelper, DocumentDBHelper>();
        services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
        services.AddSingleton<IDocumentDBClient, DocumentDBClient>();

        // shared services
        services.AddSingleton<IMessagingService, MessagingService>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();
    })
    .Build();

host.Run();
