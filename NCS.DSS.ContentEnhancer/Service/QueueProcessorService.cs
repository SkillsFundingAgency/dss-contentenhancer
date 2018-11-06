using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentEnhancer.Cosmos.Helper;
using NCS.DSS.ContentEnhancer.Models;
using Newtonsoft.Json;

namespace NCS.DSS.ContentEnhancer.Service
{
    public class QueueProcessorService
    {
        readonly string _connectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"];
        private readonly SubscriptionHelper _subscriptionHelper = new SubscriptionHelper();

        public async Task SendToTopicAsync(BrokeredMessage queueItem)
        {

            var body = new StreamReader(queueItem.GetBody<Stream>(), Encoding.UTF8).ReadToEnd();

            var messageModel = JsonConvert.DeserializeObject<MessageModel>(body);

            if (messageModel == null)
                return;

            List<Subscriptions> subscriptions;

            try
            {
                subscriptions =  await _subscriptionHelper.GetSubscriptionsAsync(messageModel);
            }
            catch (Exception ex)
            {
                throw ex.InnerException ?? ex.GetBaseException();
            }

            var doesSubscriptionExist = subscriptions != null && subscriptions.Any(x =>
                                            x.CustomerId == messageModel.CustomerGuid &&
                                            x.TouchPointId == messageModel.TouchpointId);

            if (IsANewCustomer(messageModel) && !doesSubscriptionExist)
            {
                try
                {
                    await _subscriptionHelper.CreateSubscriptionAsync(messageModel);
                }
                catch (Exception ex)
                {
                    throw ex.InnerException ?? ex.GetBaseException();
                }
                queueItem.Complete();
                return;
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
                        var message =
                            new BrokeredMessage(
                                new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel))));

                        try
                        {
                            await client.SendAsync(message);

                        }
                        catch (Exception ex)
                        {
                            throw ex.InnerException ?? ex.GetBaseException();
                        }

                    }
                }
            }
        }

        private static bool IsANewCustomer(MessageModel messageModel)
        {
            return messageModel != null && messageModel.IsNewCustomer;
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
    }
}
