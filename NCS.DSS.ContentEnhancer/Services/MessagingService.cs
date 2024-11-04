using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Models;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.ContentEnhancer.Services
{
    public class MessagingService : IMessagingService
    {
        private ServiceBusClient _client;
        private string[] _activeTouchPoints = [];

        public MessagingService()
        {
            _client = new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBusConnectionString"));
            _activeTouchPoints = Environment.GetEnvironmentVariable("ActiveTouchPoints")?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        public async Task SendMessageToTopicAsync(string topic, ILogger log, MessageModel messageModel)
        {
            await using var sender = _client.CreateSender(topic);
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)));
            message.ApplicationProperties.Add("RetryCount", 0);
            message.ApplicationProperties.Add("RetryHttpStatusCode", "");

            try
            {
                log.LogInformation($"Attempting to send message to topic: {topic}");
                await sender.SendMessageAsync(message);
                log.LogInformation($"Successfully sent message to topic: {topic}");
            }
            catch (Exception e)
            {
                log.LogError($"Failed to send message to topic. Error: {e.StackTrace}");
                throw;
            }
        }

        public string GetTopic(string touchPointId, ILogger log)
        {
            if (_activeTouchPoints != null && _activeTouchPoints.Contains(touchPointId))
            {
                return touchPointId;
            }

            log.LogWarning($"The received touchpoint ID ({touchPointId}) is invalid. Returning an empty string");
            return String.Empty;
        }

    }
}
