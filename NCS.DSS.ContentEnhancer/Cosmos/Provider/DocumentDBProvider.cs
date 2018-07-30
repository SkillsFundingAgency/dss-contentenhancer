using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.ContentEnhancer.Cosmos.Client;
using NCS.DSS.ContentEnhancer.Cosmos.Helper;
using NCS.DSS.ContentEnhancer.Cosmos.Provider;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider()
        {
            _documentDbHelper = new DocumentDBHelper();
            _databaseClient = new DocumentDBClient();
        }

        public async Task<List<Models.Subscriptions>> GetSubscriptionsByCustomerIdAsync(Guid? customerId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var Query = client
                ?.CreateDocumentQuery<Models.Subscriptions>(collectionUri)
                .Where(x => x.CustomerId == customerId)
                .AsDocumentQuery();

            if (Query == null)
                return null;

            var Subscriptions = new List<Models.Subscriptions>();

            while (Query.HasMoreResults)
            {
                var Results = await Query.ExecuteNextAsync<Models.Subscriptions>();
                Subscriptions.AddRange(Results);
            }

            return Subscriptions.Any() ? Subscriptions : null;
        }

    }
}