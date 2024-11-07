using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Services
{
    public interface ISubscriptionService
    {
        Task<List<Subscriptions>> GetSubscriptionsAsync(MessageModel messageModel, ILogger logger);
    }
}