using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ICosmosDBProvider _dbProvider;

        public SubscriptionService(ICosmosDBProvider dbProvider)
        {
            _dbProvider = dbProvider;
        }

        public async Task<List<Subscriptions>> GetSubscriptionsAsync(MessageModel messageModel, ILogger logger)
        {
            var customerGuid = messageModel.CustomerGuid;
            var senderTouchPointId = messageModel.TouchpointId;

            logger.LogTrace($"Attempting to retrieve SUBSCRIPTIONS, which have a different touchpoint ID to {senderTouchPointId}, for Customer with GUID: {customerGuid}");
            List<Subscriptions> subscriptions = await _dbProvider.GetSubscriptionsByCustomerIdAsync(customerGuid, senderTouchPointId);

            if (subscriptions == null)
            {
                logger.LogInformation($"No subscriptions found, which have a different touchpoint ID to {senderTouchPointId}, for Customer with GUID: {customerGuid}");
            }

            return subscriptions;
        }
    }
}
