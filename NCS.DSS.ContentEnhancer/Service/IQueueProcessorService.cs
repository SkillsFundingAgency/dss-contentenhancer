using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Models;
using System.Threading.Tasks;

namespace NCS.DSS.ContentEnhancer.Service
{
    public interface IQueueProcessorService
    {
        Task SendToTopicAsync(MessageModel message, ILogger log);
    }
}