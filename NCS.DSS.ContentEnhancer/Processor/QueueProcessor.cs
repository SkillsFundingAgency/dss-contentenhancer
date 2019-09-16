using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Service;

namespace NCS.DSS.ContentEnhancer.Processor
{
    public class QueueProcessor
    {
        private readonly IQueueProcessorService _queueProcessorService;

        public QueueProcessor(IQueueProcessorService queueProcessorService)
        {
            _queueProcessorService = queueProcessorService;
        }

        [FunctionName("QueueProcessor")]
        public async System.Threading.Tasks.Task RunAsync([ServiceBusTrigger("dss.contentqueue", Connection = "ServiceBusConnectionString")]Message queueItem, ILogger log)
        {
            if (queueItem == null)
            {
                log.LogError("Brokered Message cannot be null");
                throw new ArgumentNullException(nameof(queueItem));
            }

            try
            {
                log.LogInformation("attempting to call processor service");
                await _queueProcessorService.SendToTopicAsync(queueItem, log);
            }
            catch (Exception ex)
            {
                log.LogError(ex.StackTrace);
                throw;
            }
        }
    }
}
