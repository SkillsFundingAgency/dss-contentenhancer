using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.ContentEnhancer.Cosmos.Helper
{
    public class SubscriptionHelper : ISubscriptionHelper
    {
        private readonly IDocumentDBProvider _dbProvider;

        public SubscriptionHelper(IDocumentDBProvider dbProvider)
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

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : (Guid?)null;
        }

        public async Task<List<Subscriptions>> GetSubscriptionsAsync(MessageModel messageModel, ILogger logger)
        {
            logger.LogInformation("Getting Subscription Async");

            if (messageModel == null)
                return null;


            var customerGuid = messageModel.CustomerGuid;
            var senderTouchPointId = messageModel.TouchpointId;

            logger.LogInformation("Getting Subscription From DB");

            var subscriptions = await _dbProvider.GetSubscriptionsByCustomerIdAsync(customerGuid, senderTouchPointId);

            if (subscriptions != null)
                logger.LogInformation(string.Format("Retrieved {0} Subscriptions From DB ", subscriptions.Count));

            return subscriptions;
        }

    }
}
