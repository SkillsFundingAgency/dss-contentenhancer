using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Azure;
using NCS.DSS.ContentEnhancer.Service;
using System;

namespace NCS.DSS.ContentEnhancer
{
    public static class QueueProcessor
    {
        [FunctionName("QueueProcessor")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("dss.contentqueue", AccessRights.Manage, Connection = "ServiceBusConnectionString")]string queueItem, 
            TraceWriter log)
        {
            QueueProcessorService service = new QueueProcessorService();
            await service.SendToTopicAsync(queueItem);
        }
    }
}
