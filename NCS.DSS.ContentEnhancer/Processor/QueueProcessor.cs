using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Models;
using NCS.DSS.ContentEnhancer.Services;
namespace NCS.DSS.ContentEnhancer.Processor
{
    public class QueueProcessor : IQueueProcessor
    {
        private readonly ILogger<QueueProcessor> _logger;
        private readonly IMessagingService _messagingService;
        private readonly ISubscriptionService _subscriptionService;

        public QueueProcessor(ILogger<QueueProcessor> logger, IMessagingService messagingService, ISubscriptionService subscriptionService)
        {
            _logger = logger;
            _messagingService = messagingService;
            _subscriptionService = subscriptionService;
        }

        [Function("QueueProcessor")]
        public async Task RunAsync([ServiceBusTrigger("dss.contentqueue", Connection = "ServiceBusConnectionString")] MessageModel message)
        {
            _logger.LogInformation($"Function {nameof(QueueProcessor)} has been invoked");

            if (message == null)
            {
                _logger.LogError("Request message received was NULL");
                throw new ArgumentNullException(nameof(message));
            }

            bool messageIsDataCollections = message.DataCollections.HasValue && message.DataCollections == true;
            if (messageIsDataCollections)
            {
                string topic = _messagingService.GetTopic(message.TouchpointId, _logger);

                if (topic == String.Empty)
                {
                    _logger.LogError($"Invalid or unsupported touchpoint ID: {message.TouchpointId}");
                    throw new ArgumentException($"Invalid or unsupported touchpoint ID: {message.TouchpointId}");
                }

                _logger.LogInformation("Data Collections related message has been received");
                await _messagingService.SendMessageToTopicAsync(topic, _logger, message);
                return;
            }

            List<Subscriptions> subscriptions;

            try
            {
                // retrieves subscriptions for a given customer which AREN'T associated with the originating Touchpoint ID
                subscriptions = await _subscriptionService.GetSubscriptionsAsync(message, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to retrieve SUBSCRIPTIONS for Customer. Error: {ex.StackTrace}");
                throw;
            }

            bool messageIncludesDigitalIdentity = message.IsDigitalAccount.GetValueOrDefault();
            if (messageIncludesDigitalIdentity)
            {
                string digitalIdentitiesTopic = Environment.GetEnvironmentVariable("DigitalIdentitiesTopic");
                await _messagingService.SendMessageToTopicAsync(digitalIdentitiesTopic, _logger, message);
            }

            if (subscriptions == null || subscriptions.Count == 0) // should it be possible for someone to not have an active subscription?
            {
                _logger.LogError($"Failed to retrieve SUBSCRIPTIONS for Customer. Customer GUID: {message.CustomerGuid}. Touchpoint ID: {message.TouchpointId}");
            }
            else 
            {
                _logger.LogInformation($"Subscription count: {subscriptions.Count}");

                foreach (var subscription in subscriptions)
                {
                    string topic = _messagingService.GetTopic(subscription.TouchPointId, _logger);

                    if (topic == String.Empty)
                    {
                        _logger.LogWarning($"Invalid or unsupported subscription touchpoint ID: {subscription.TouchPointId}");
                        continue;
                    }

                    _logger.LogInformation("Subscription notification related message has been received");
                    await _messagingService.SendMessageToTopicAsync(topic, _logger, message);
                }
            }

            _logger.LogInformation($"Function {nameof(QueueProcessor)} has finished invoking");
        }
    }
}
