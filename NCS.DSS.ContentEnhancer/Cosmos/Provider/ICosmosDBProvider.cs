using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Cosmos.Provider
{
    public interface ICosmosDBProvider
    {
        Task<List<Subscriptions>> GetSubscriptionsByCustomerIdAsync(Guid? customerId, string senderTouchpointId);
    }
}