using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Cosmos.Helper;
using NCS.DSS.ContentEnhancer.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using System.Linq;

namespace NCS.DSS.ContentEnhancer.Service
{
    public class QueueProcessorService : IQueueProcessorService
    {
        readonly string _connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        private readonly ISubscriptionHelper _subscriptionHelper;
        private readonly TouchpointTopics _touchpoints;
        readonly string _digitalIdentitiesTopic = Environment.GetEnvironmentVariable("DigitalIdentitiesTopic");

        public QueueProcessorService(ISubscriptionHelper subscriptionHelper, IOptions<TouchpointTopics> options)
        {
            _subscriptionHelper = subscriptionHelper;
            _touchpoints = options.Value;
        }

        public async Task SendToTopicAsync(Message queueItem, ILogger log)
        {
            log.LogInformation("Entered SendToTopicAsync");

            var body = string.Empty;

            log.LogInformation("got body from stream reader");


            try
            {
                body = Encoding.UTF8.GetString(queueItem.Body);
            }
            catch (Exception ex)
            {
                log.LogError("Unable to retrieve body from Message", ex.Message);
            }

            MessageModel messageModel;

            log.LogInformation("Deserialize body into message model");

            try
            {
                messageModel = JsonConvert.DeserializeObject<MessageModel>(body);
            }
            catch (JsonException ex)
            {
                log.LogError("Unable to retrieve body from req", ex.Message);
                throw;
            }

            if (messageModel == null)
                return;

            //Bypass subscriptions logic for DataCollections Messages
            if (messageModel.DataCollections.HasValue && messageModel.DataCollections == true)
            {
                log.LogInformation("Send Message Async to Topic");
                await SendMessageToTopicAsync(GetTopic(messageModel.TouchpointId, log), log, messageModel);
                return;
            }

            List<Subscriptions> subscriptions;

            try
            {
                log.LogInformation("getting subscriptions for customer");
                //Get all subscriptions for a customer where touchpointID is not equal to the senders touchpoint id
                subscriptions = await _subscriptionHelper.GetSubscriptionsAsync(messageModel, log);
            }
            catch (Exception ex)
            {
                log.LogError("Get Subscriptions Error: " + ex.StackTrace);
                throw;
            }

            // If source of data came from DigitalIdentity service then send message to digitalidentities topic
            if (messageModel.IsDigitalAccount.GetValueOrDefault())
            {
                await SendMessageToTopicAsync(_digitalIdentitiesTopic, log, messageModel);
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

                        await SendMessageToTopicAsync(topic, log, messageModel);
                    }
                }
            }
        }

        private async Task SendMessageToTopicAsync(string topic, ILogger log, MessageModel messageModel)
        {
            var client = new TopicClient(_connectionString, topic);
            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)));

            message.UserProperties.Add("RetryCount", 0);
            message.UserProperties.Add("RetryHttpStatusCode", "");

            try
            {
                log.LogInformation("sending message to topic");
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                log.LogError("Send Message To Topic Error: " + ex.StackTrace);
                await client.CloseAsync();
                throw;
            }
            finally
            {
                if (!client.IsClosedOrClosing)
                    await client.CloseAsync();
            }
            
        }

        private  string GetTopic(string touchPointId, ILogger log)
        {
            var enabledTouchPoints = _touchpoints.EnabledTouchPoints;

            if (enabledTouchPoints != null && enabledTouchPoints.Contains(touchPointId))
            {         
                return touchPointId;
            }

            log.LogWarning("Touchpoint {0} invalid, returning empty string", touchPointId);
            return String.Empty;
        }
    }
}
