using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentEnhancer.Service;

namespace NCS.DSS.ContentEnhancer.Processor
{
    public static class QueueProcessor
    {
        [FunctionName("QueueProcessor")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("dss.contentqueue", AccessRights.Manage, Connection = "ServiceBusConnectionString")]BrokeredMessage queueItem, 
            TraceWriter log)
        {
            if (queueItem == null)
            {
                log.Error("Brokered Message cannot be null");
                return;
            }

            var service = new QueueProcessorService();
            try
            {
                await service.SendToTopicAsync(queueItem);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                throw;
            }

        }
    }
}
