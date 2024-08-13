using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.ContentEnhancer.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<List<Models.Subscriptions>> GetSubscriptionsByCustomerIdAsync(Guid? customerId, string SenderTouchpointId);
        Task<ResourceResponse<Document>> CreateSubscriptionsAsync(Models.Subscriptions subscriptions);
    }
}