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

namespace NCS.DSS.ContentEnhancer.Service
{
    public class QueueProcessorService : IQueueProcessorService
    {
        readonly string _connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        private readonly ISubscriptionHelper _subscriptionHelper;
        readonly string _digitalIdentitiesTopic = Environment.GetEnvironmentVariable("DigitalIdentitiesTopic");

        public QueueProcessorService(ISubscriptionHelper subscriptionHelper)
        {
            _subscriptionHelper = subscriptionHelper;
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
                await SendMessageToTopicAsync(GetTopic(messageModel.TouchpointId), log, messageModel);
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
                        var topic = GetTopic(subscription.TouchPointId);

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

        private static string GetTopic(string touchPointId)
        {
            switch (touchPointId)
            {
                case "0000000101":
                    return "eastandbucks";
                case "0000000102":
                    return "eastandnorthampton";
                case "0000000103":
                    return "london";
                case "0000000104":
                    return "westmidsandstaffs";
                case "0000000105":
                    return "northwest";
                case "0000000106":
                    return "northeastandcumbria";
                case "0000000107":
                    return "southeast";
                case "0000000108":
                    return "southwestandoxford";
                case "0000000109":
                    return "yorkshireandhumber";
                case "0000000999":
                    return "careershelpline";


                ////////////////////////////////////
                ///////For test team use only///////
                case "9000000000":
                    return "dss-test-touchpoint-1";
                case "9111111111":
                    return "dss-test-touchpoint-2";
                case "9222222222":
                    return "dss-test-touchpoint-3";
                ////////////////////////////////////

                default:
                    return string.Empty;
            }
        }
    }
}
