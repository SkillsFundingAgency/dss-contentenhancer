using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Azure;
using System.Net;
using System.Runtime.Serialization.Json;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using Newtonsoft.Json;
using NCS.DSS.ContentEnhancer.Models;
using System.Configuration;

namespace NCS.DSS.ContentEnhancer.Service
{
    public class QueueProcessorService
    {
        readonly string connectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"];

        public async Task SendToTopicAsync(string queueItem)
        {
            try
            {
                List<Models.Subscriptions> Sub = await GetSubscriptionsAsync(queueItem);

                if (Sub != null)
                {
                    if (Sub.Count() != 0)
                    {
                        foreach (var subscription in Sub)
                        {
                            string topic = GetTopic();
                            TopicClient Client = TopicClient.CreateFromConnectionString(connectionString, topic);
                            BrokeredMessage message = new BrokeredMessage(queueItem);
                            message.ContentType = "application/json";
                            await Client.SendAsync(message);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public string GetTopic()
        {
            //Require logic to get the TOPIC name to send the message to
            //
            //
            //
            ////

            return "eastandnorthampton";
        }
        
        public async Task<List<Models.Subscriptions>> GetSubscriptionsAsync(string queueItem)
        {
            MessageModel customer = JsonConvert.DeserializeObject<MessageModel>(queueItem);
            Guid? customerGuid = customer.CustomerGuid;

            var documentDbProvider = new DocumentDBProvider();
            List<Models.Subscriptions> subscriptions = await documentDbProvider.GetSubscriptionsByCustomerIdAsync(customerGuid);

            return subscriptions;
        }

    }

}
