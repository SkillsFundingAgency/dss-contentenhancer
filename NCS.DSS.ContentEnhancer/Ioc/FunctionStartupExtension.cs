using Microsoft.Azure.Documents;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.ContentEnhancer.Cosmos.Client;
using NCS.DSS.ContentEnhancer.Cosmos.Helper;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Ioc;
using NCS.DSS.ContentEnhancer.Service;

[assembly: FunctionsStartup(typeof(FunctionStartupExtension))]

namespace NCS.DSS.ContentEnhancer.Ioc
{
    public class FunctionStartupExtension : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ISubscriptionHelper, SubscriptionHelper>();
            builder.Services.AddSingleton<IQueueProcessorService, QueueProcessorService>();
            builder.Services.AddSingleton<IDocumentDBHelper, DocumentDBHelper>();
            builder.Services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
            builder.Services.AddSingleton<IDocumentDBClient, DocumentDBClient>();
        }
    }
}
