using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Models;
using Newtonsoft.Json;

namespace NCS.DSS.ContentEnhancer.Service
{
    public class QueueProcessorService
    {
        readonly string _connectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"];

        public async Task SendToTopicAsync(string queueItem)
        {
            try
            {
                var subscriptions = await GetSubscriptionsAsync(queueItem);

                if (subscriptions != null)
                {
                    if (subscriptions.Count != 0)
                    {
                        foreach (var subscription in subscriptions)
                        {
                            var topic = GetTopic(subscription.TouchPointId);

                            if (string.IsNullOrWhiteSpace(topic))
                                continue;

                            var client = TopicClient.CreateFromConnectionString(_connectionString, topic);
                            var message = new BrokeredMessage(queueItem) {ContentType = "application/json"};
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

        public string GetTopic(string touchPointId)
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

        public async Task<List<Subscriptions>> GetSubscriptionsAsync(string queueItem)
        {
            var customer = JsonConvert.DeserializeObject<MessageModel>(queueItem);
            var customerGuid = customer.CustomerGuid;

            var documentDbProvider = new DocumentDBProvider();
            var subscriptions = await documentDbProvider.GetSubscriptionsByCustomerIdAsync(customerGuid);

            return subscriptions;
        }

    }

}
