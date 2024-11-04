using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IDocumentDBProvider _dbProvider;

        public SubscriptionService(IDocumentDBProvider dbProvider)
        {
            _dbProvider = dbProvider;
        }

        public async Task<List<Subscriptions>> GetSubscriptionsAsync(MessageModel messageModel, ILogger logger)
        {
            var customerGuid = messageModel.CustomerGuid;
            var senderTouchPointId = messageModel.TouchpointId;

            logger.LogInformation($"Attempting to retrieve SUBSCRIPTIONS, which have a different touchpoint ID to {senderTouchPointId}, for Customer with GUID: {customerGuid}");
            List<Subscriptions> subscriptions = await _dbProvider.GetSubscriptionsByCustomerIdAsync(customerGuid, senderTouchPointId);

            if (subscriptions != null)
            {
                logger.LogInformation($"Successfully retrieved {subscriptions.Count} subscriptions from Cosmos DB");
            }

            return subscriptions;
        }
    }
}
