using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Cosmos.Helper
{
    public interface ISubscriptionHelper
    {
        Task<Subscriptions> CreateSubscriptionAsync(MessageModel messageModel);
        Task<List<Subscriptions>> GetSubscriptionsAsync(MessageModel messageModel);
    }
}