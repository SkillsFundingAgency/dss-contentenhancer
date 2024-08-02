using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Cosmos.Helper;
using NCS.DSS.ContentEnhancer.Models;
using Newtonsoft.Json;
using System.Linq;
using Azure.Messaging.ServiceBus;

namespace NCS.DSS.ContentEnhancer.Service
{
    public class QueueProcessorService : IQueueProcessorService
    {
        readonly string _connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        private readonly ISubscriptionHelper _subscriptionHelper;
        readonly string _digitalIdentitiesTopic = Environment.GetEnvironmentVariable("DigitalIdentitiesTopic");
        private readonly string[] _activeTouchPoints = Environment.GetEnvironmentVariable("ActiveTouchPoints")
            ?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        private readonly ServiceBusClient _client;

        public QueueProcessorService(ISubscriptionHelper subscriptionHelper)
        {
            _subscriptionHelper = subscriptionHelper;
            _client = new ServiceBusClient(_connectionString);
        }

        public async Task SendToTopicAsync(MessageModel message, ILogger log)
        {
            log.LogInformation("Entered SendToTopicAsync");


            if (message == null)
                return;

            //Bypass subscriptions logic for DataCollections Messages
            if (message.DataCollections.HasValue && message.DataCollections == true)
            {
                log.LogInformation("Send Message Async to Topic");
                await SendMessageToTopicAsync(GetTopic(message.TouchpointId, log), log, message);
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
                await SendMessageToTopicAsync(_digitalIdentitiesTopic, log, message);
            }


            //For each subscription - send notification
            if (subscriptions != null)
            {
                if (subscriptions.Count != 0)
                {
                    log.LogInformation(string.Format("subscription count: {0} ", subscriptions.Count));

                    foreach (var subscription in subscriptions)
                    {
                        var topic = GetTopic(subscription.TouchPointId, log);

                        if (string.IsNullOrWhiteSpace(topic))
                            continue;

                        log.LogInformation(string.Format("Send Message to Topic {0}", topic));

                        await SendMessageToTopicAsync(topic, log, message);
                    }
                }
            }
        }

        private async Task SendMessageToTopicAsync(string topic, ILogger log, MessageModel messageModel)
        {
            await using var sender = _client.CreateSender(topic);
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)));
            message.ApplicationProperties.Add("RetryCount", 0);
            message.ApplicationProperties.Add("RetryHttpStatusCode", "");

            try
            {
                log.LogInformation("sending message to topic {0}", topic);
                await sender.SendMessageAsync(message);
            }
            catch (Exception e)
            {
                log.LogError("Send Message To Topic Error: " + e.StackTrace);
                throw;
            }
        }

        private  string GetTopic(string touchPointId, ILogger log)
        {
            if (_activeTouchPoints != null && _activeTouchPoints.Contains(touchPointId))
            {         
                return touchPointId;
            }

            log.LogWarning("Touchpoint {0} invalid, returning empty string", touchPointId);
            return String.Empty;
        }
    }
}
