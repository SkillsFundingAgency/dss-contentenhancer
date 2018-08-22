using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

using NCS.DSS.ContentEnhancer.Service;

namespace NCS.DSS.ContentEnhancer
{
    public static class QueueProcessor
    {
        [FunctionName("QueueProcessor")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("dss.contentqueue", AccessRights.Manage, Connection = "ServiceBusConnectionString")]BrokeredMessage queueItem, 
            TraceWriter log)
        {
            QueueProcessorService service = new QueueProcessorService();
            await service.SendToTopicAsync(queueItem);
        }
    }
}
