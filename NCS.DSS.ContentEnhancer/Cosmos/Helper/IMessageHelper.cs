using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Service
{
    public interface IMessageHelper
    {
        Task SendMessageToTopicAsync(string topic, ILogger log, MessageModel messageModel);
        string GetTopic(string touchPointId, ILogger log);
    }
}