using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.ContentEnhancer.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<List<Models.Subscriptions>> GetSubscriptionsByCustomerIdAsync(Guid? customerId);
    }
}