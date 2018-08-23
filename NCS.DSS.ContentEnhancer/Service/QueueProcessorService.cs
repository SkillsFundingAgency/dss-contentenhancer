using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Models;
using Newtonsoft.Json;

namespace NCS.DSS.ContentEnhancer.Service
{
    public class QueueProcessorService
    {
        readonly string _connectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"];

        public async Task SendToTopicAsync(BrokeredMessage queueItem)
        {
            try
            {

                var body = new StreamReader(queueItem.GetBody<Stream>(), Encoding.UTF8).ReadToEnd();

                var messageModel = JsonConvert.DeserializeObject<MessageModel>(body);

                if (messageModel == null)
                    return;

                var subscriptions = await GetSubscriptionsAsync(messageModel);

                var doesSubscriptionExist = subscriptions != null && subscriptions.Any(x =>
                                                x.CustomerId == messageModel.CustomerGuid &&
                                                x.TouchPointId == messageModel.TouchpointId);

                if (IsANewCustomer(messageModel) && !doesSubscriptionExist)
                {
                    await CreateSubscriptionAsync(messageModel);
                }

                if (subscriptions != null)
                {
                    if (subscriptions.Count != 0)
                    {
                        foreach (var subscription in subscriptions)
                        {
                            if (messageModel.TouchpointId == subscription.TouchPointId)
                                continue;

                            var topic = GetTopic(subscription.TouchPointId);

                            if (string.IsNullOrWhiteSpace(topic))
                                continue;

                            var client = TopicClient.CreateFromConnectionString(_connectionString, topic);
                            var message = new BrokeredMessage(queueItem.Clone());
                            await client.SendAsync(message);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
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
                default:
                    return string.Empty;
            }
        }

        public bool IsANewCustomer(MessageModel messageModel)
        {
            return messageModel != null && messageModel.IsNewCustomer;
        }

        public async Task<Subscriptions> CreateSubscriptionAsync(MessageModel messageModel)
        {
            if (messageModel == null)
                return null;

            var subcription = new Subscriptions
            {
                SubscriptionId = Guid.NewGuid(),
                CustomerId = messageModel.CustomerGuid.GetValueOrDefault(),
                TouchPointId = messageModel.TouchpointId,
                Subscribe = true,
                LastModifiedDate = messageModel.LastModifiedDate,
                
            };

            if (!messageModel.LastModifiedDate.HasValue)
                subcription.LastModifiedDate = DateTime.Now;

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateSubscriptionsAsync(subcription);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : (Guid?)null;
        }

        public async Task<List<Subscriptions>> GetSubscriptionsAsync(MessageModel messageModel)
        {
            if (messageModel == null)
                return null;

            var customerGuid = messageModel.CustomerGuid;

            var documentDbProvider = new DocumentDBProvider();
            var subscriptions = await documentDbProvider.GetSubscriptionsByCustomerIdAsync(customerGuid);

            return subscriptions;
        }

    }

}
