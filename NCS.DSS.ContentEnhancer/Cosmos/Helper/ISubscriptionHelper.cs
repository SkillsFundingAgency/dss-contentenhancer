using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.ContentEnhancer.Cosmos.Helper
{
    public interface ISubscriptionHelper
    {
        Task<Subscriptions> CreateSubscriptionAsync(MessageModel messageModel, ILogger logger);
        Task<List<Subscriptions>> GetSubscriptionsAsync(MessageModel messageModel, ILogger logger);
    }
}