using Microsoft.Azure.Cosmos;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<List<Subscriptions>> GetSubscriptionsByCustomerIdAsync(Guid? customerId, string senderTouchpointId);
        Task<ItemResponse<Subscriptions>> CreateSubscriptionsAsync(Subscriptions subscriptions);
    }
}