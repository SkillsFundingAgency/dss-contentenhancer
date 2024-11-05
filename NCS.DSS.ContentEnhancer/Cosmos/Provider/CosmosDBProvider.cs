using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Cosmos.Provider
{
    public class CosmosDBProvider : ICosmosDBProvider
    {
        private readonly Container _container;
        private readonly string _databaseId = Environment.GetEnvironmentVariable("DatabaseId");
        private readonly string _containerId = Environment.GetEnvironmentVariable("CollectionId");

        public CosmosDBProvider(CosmosClient cosmosClient)
        {
            _container = cosmosClient.GetContainer(_databaseId, _containerId);
        }

        public async Task<List<Subscriptions>> GetSubscriptionsByCustomerIdAsync(Guid? customerId, string senderTouchPointId)
        {
            if (customerId == null)
            {
                return null;
            }

            var query = _container.GetItemLinqQueryable<Subscriptions>(true)
                .Where(x => x.CustomerId == customerId && x.TouchPointId != senderTouchPointId && x.Subscribe)
                .ToFeedIterator();

            List<Subscriptions> subscriptions = new List<Subscriptions>();

            while (query.HasMoreResults)
            {
                var results = await query.ReadNextAsync();
                subscriptions.AddRange(results);
            }

            return subscriptions.Any() ? subscriptions : null;
        }
        public async Task<ItemResponse<Subscriptions>> CreateSubscriptionsAsync(Subscriptions subscriptions)
        {
            return await _container.CreateItemAsync(subscriptions, new PartitionKey(subscriptions.CustomerId.ToString()));
        }
    }
}