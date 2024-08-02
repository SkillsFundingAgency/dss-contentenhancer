using System;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Models;
using NCS.DSS.ContentEnhancer.Service;
using Microsoft.Azure.Functions.Worker;

namespace NCS.DSS.ContentEnhancer.Processor
{
    public class QueueProcessor
    {
        private readonly IQueueProcessorService _queueProcessorService;
        private readonly ILogger<QueueProcessor> _logger;

        public QueueProcessor(IQueueProcessorService queueProcessorService, ILogger<QueueProcessor> logger)
        {
            _queueProcessorService = queueProcessorService;
            _logger = logger;
        }

        [Function("QueueProcessor")]
        public async System.Threading.Tasks.Task RunAsync([ServiceBusTrigger("dss.contentqueue", Connection = "ServiceBusConnectionString")]MessageModel message)
        {
            if (message == null)
            {
                _logger.LogError("Brokered Message cannot be null");
                throw new ArgumentNullException(nameof(message));
            }

            try
            {
                _logger.LogInformation("attempting to call processor service");
                await _queueProcessorService.SendToTopicAsync(message, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                throw;
            }
        }
    }
}
