using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.ContentEnhancer;
using NCS.DSS.ContentEnhancer.Cosmos.Client;
using NCS.DSS.ContentEnhancer.Cosmos.Helper;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Models;
using NCS.DSS.ContentEnhancer.Service;

[assembly: FunctionsStartup(typeof(Startup))]

namespace NCS.DSS.ContentEnhancer
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ISubscriptionHelper, SubscriptionHelper>();
            builder.Services.AddSingleton<IQueueProcessorService, QueueProcessorService>();
            builder.Services.AddSingleton<IDocumentDBHelper, DocumentDBHelper>();
            builder.Services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
            builder.Services.AddSingleton<IDocumentDBClient, DocumentDBClient>();
            builder.Services.AddOptions<TouchpointTopics>()
                .Configure<IConfiguration>((settings, configuration) =>
                {     
                    configuration.GetSection("TouchpointTopics").Bind(settings); 
                });
        }
    }
}
