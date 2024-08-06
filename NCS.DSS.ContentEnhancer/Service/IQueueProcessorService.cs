using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Service
{
    public interface IQueueProcessorService
    {
        Task SendToTopicAsync(MessageModel message, ILogger log);        
    }
}