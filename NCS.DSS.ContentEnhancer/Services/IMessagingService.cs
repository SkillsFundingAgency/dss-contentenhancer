using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Services
{
    public interface IMessagingService
    {
        Task SendMessageToTopicAsync(string topic, ILogger log, MessageModel messageModel);
        string GetTopic(string touchPointId, ILogger log);
    }
}