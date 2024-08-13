using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Cosmos.Helper;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Service
{
    public class QueueProcessorService : IQueueProcessorService
    {
        private readonly ISubscriptionHelper _subscriptionHelper;
        private readonly string _digitalIdentitiesTopic = Environment.GetEnvironmentVariable("DigitalIdentitiesTopic");
        private readonly IMessageHelper _messageHelper;

        public QueueProcessorService(ISubscriptionHelper subscriptionHelper, IMessageHelper messageHelper)
        {
            _subscriptionHelper = subscriptionHelper;
            _messageHelper = messageHelper;
        }

        public async Task SendToTopicAsync(MessageModel message, ILogger log)
        {
            log.LogInformation("Entered SendToTopicAsync");


            if (message == null)
                return;

            //Bypass subscriptions logic for DataCollections Messages
            if (message.DataCollections.HasValue && message.DataCollections == true)
            {
                var topic = _messageHelper.GetTopic(message.TouchpointId, log);
                log.LogInformation("Data Collections - Send Message to Topic {0}", topic);
                await _messageHelper.SendMessageToTopicAsync(topic, log, message);
                return;
            }

            List<Subscriptions> subscriptions;

            try
            {
                log.LogInformation("getting subscriptions for customer");
                //Get all subscriptions for a customer where touchpointID is not equal to the senders touchpoint id
                subscriptions = await _subscriptionHelper.GetSubscriptionsAsync(message, log);
            }
            catch (Exception ex)
            {
                log.LogError("Get Subscriptions Error: " + ex.StackTrace);
                throw;
            }

            // If source of data came from DigitalIdentity service then send message to digitalidentities topic
            if (message.IsDigitalAccount.GetValueOrDefault())
            {
                await _messageHelper.SendMessageToTopicAsync(_digitalIdentitiesTopic, log, message);
            }


            //For each subscription - send notification
            if (subscriptions != null)
            {
                if (subscriptions.Count != 0)
                {
                    log.LogInformation(string.Format("subscription count: {0} ", subscriptions.Count));

                    foreach (var subscription in subscriptions)
                    {
                        var topic = _messageHelper.GetTopic(subscription.TouchPointId, log);

                        if (string.IsNullOrWhiteSpace(topic))
                            continue;

                        log.LogInformation("Subscriptions - Send Message to Topic {0}", topic);

                        await _messageHelper.SendMessageToTopicAsync(topic, log, message);
                    }
                }
            }
            else
                log.LogError("Failed to get subscriptions");
        }

    }
}
