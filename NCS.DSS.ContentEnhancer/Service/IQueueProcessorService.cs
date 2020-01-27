using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentEnhancer.Service
{
    public interface IQueueProcessorService
    {
        Task SendToTopicAsync(Message queueItem, ILogger log);
    }
}