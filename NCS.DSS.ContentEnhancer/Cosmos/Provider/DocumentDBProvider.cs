﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.ContentEnhancer.Cosmos.Client;
using NCS.DSS.ContentEnhancer.Cosmos.Helper;

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

            var query = client
                ?.CreateDocumentQuery<Models.Subscriptions>(collectionUri)
                .Where(x => x.CustomerId == customerId &&
                            x.Subscribe)
                .AsDocumentQuery();

            if (query == null)
                return null;

            var subscriptions = new List<Models.Subscriptions>();

            while (query.HasMoreResults)
            {
                var results = await query.ExecuteNextAsync<Models.Subscriptions>();
                subscriptions.AddRange(results);
            }

            return subscriptions.Any() ? subscriptions : null;
        }

        public async Task<ResourceResponse<Document>> CreateSubscriptionsAsync(Models.Subscriptions subscriptions)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, subscriptions);

            return response;

        }

    }
}