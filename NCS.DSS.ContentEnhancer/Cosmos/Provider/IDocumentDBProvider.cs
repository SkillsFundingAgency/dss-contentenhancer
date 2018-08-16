using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.ContentEnhancer.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<List<Models.Subscriptions>> GetSubscriptionsByCustomerIdAsync(Guid? customerId);
        Task<ResourceResponse<Document>> CreateSubscriptionsAsync(Models.Subscriptions subscriptions);
    }
}