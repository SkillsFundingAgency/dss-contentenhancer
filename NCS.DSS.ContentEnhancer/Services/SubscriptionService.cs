using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Models;
using System.Net;

namespace NCS.DSS.ContentEnhancer.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IDocumentDBProvider _dbProvider;

        public SubscriptionService(IDocumentDBProvider dbProvider)
        {
            _dbProvider = dbProvider;
        }

        public async Task<Subscriptions> CreateSubscriptionAsync(MessageModel messageModel, ILogger logger)
        {
            logger.LogInformation("Creating Subscription Async");

            if (messageModel == null)
                return null;

            var subscription = new Subscriptions
            {
                SubscriptionId = Guid.NewGuid(),
                CustomerId = messageModel.CustomerGuid.GetValueOrDefault(),
                TouchPointId = messageModel.TouchpointId,
                Subscribe = true,
                LastModifiedDate = messageModel.LastModifiedDate,

            };

            if (!messageModel.LastModifiedDate.HasValue)
                subscription.LastModifiedDate = DateTime.Now;

            logger.LogInformation("Creating Subscription In DB");
            var response = await _dbProvider.CreateSubscriptionsAsync(subscription);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task<List<Subscriptions>> GetSubscriptionsAsync(MessageModel messageModel, ILogger logger)
        {
            var customerGuid = messageModel.CustomerGuid;
            var senderTouchPointId = messageModel.TouchpointId;

            logger.LogInformation($"Attempting to retrieve SUBSCRIPTIONS for Customer. Customer GUID: {customerGuid}. Touchpoint ID: {senderTouchPointId}");
            List<Subscriptions> subscriptions = await _dbProvider.GetSubscriptionsByCustomerIdAsync(customerGuid, senderTouchPointId);

            if (subscriptions != null)
            {
                logger.LogInformation($"Successfully retrieved {subscriptions.Count} subscriptions from CosmosDB");
            }

            return subscriptions;
        }

    }
}
