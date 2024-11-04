using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.ContentEnhancer.Cosmos.Client;
using NCS.DSS.ContentEnhancer.Cosmos.Helper;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly IDocumentDBHelper _documentDbHelper;
        private readonly IDocumentDBClient _databaseClient;

        public DocumentDBProvider(IDocumentDBHelper documentDbHelper, IDocumentDBClient documentDbClient)
        {
            _documentDbHelper = documentDbHelper;
            _databaseClient = documentDbClient;
        }

        public async Task<List<Subscriptions>> GetSubscriptionsByCustomerIdAsync(Guid? customerId, string senderTouchPointId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();
            var client = _databaseClient.CreateDocumentClient();

            var query = client?.CreateDocumentQuery<Subscriptions>(collectionUri).Where(
                x => x.CustomerId == customerId && x.TouchPointId != senderTouchPointId && x.Subscribe).AsDocumentQuery();

            if (query == null)
            {
                return null;
            }

            List<Subscriptions> subscriptions = new List<Subscriptions>();

            while (query.HasMoreResults)
            {
                var results = await query.ExecuteNextAsync<Subscriptions>();
                subscriptions.AddRange(results);
            }

            return subscriptions.Any() ? subscriptions : null;
        }

        public async Task<ResourceResponse<Document>> CreateSubscriptionsAsync(Subscriptions subscriptions)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
            {
                return null;
            }

            var response = await client.CreateDocumentAsync(collectionUri, subscriptions);
            return response;
        }
    }
}