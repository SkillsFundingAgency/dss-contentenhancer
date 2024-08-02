using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.ContentEnhancer.Cosmos.Client;
using NCS.DSS.ContentEnhancer.Cosmos.Helper;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Service;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices( services =>
    {
        services.AddSingleton<ISubscriptionHelper, SubscriptionHelper>();
        services.AddSingleton<IQueueProcessorService, QueueProcessorService>();
        services.AddSingleton<IDocumentDBHelper, DocumentDBHelper>();
        services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
        services.AddSingleton<IDocumentDBClient, DocumentDBClient>();
    })
    .Build();

host.Run();
