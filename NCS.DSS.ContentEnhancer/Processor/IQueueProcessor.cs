using Microsoft.Azure.Functions.Worker;
using NCS.DSS.ContentEnhancer.Models;

namespace NCS.DSS.ContentEnhancer.Processor
{
    public interface IQueueProcessor
    {
        Task RunAsync([ServiceBusTrigger("dss.contentqueue", Connection = "ServiceBusConnectionString")] MessageModel message);
    }
}
