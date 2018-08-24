using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Cosmos.Helper
{
    public class SubscriptionHelper : ISubscriptionHelper
    {
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
