using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Models;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.ContentEnhancer.Service
{
    public class MessageHelper : IMessageHelper
    {

        private ServiceBusClient _client;
        private string[] _activeTouchPoints = [];

        public MessageHelper()
        {
            _client = new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBusConnectionString"));
            _activeTouchPoints = Environment.GetEnvironmentVariable("ActiveTouchPoints")
            ?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        public async Task SendMessageToTopicAsync(string topic, ILogger log, MessageModel messageModel)
        {
            await using var sender = _client.CreateSender(topic);
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)));
            message.ApplicationProperties.Add("RetryCount", 0);
            message.ApplicationProperties.Add("RetryHttpStatusCode", "");

            try
            {
                log.LogInformation("sending message to topic {0}", topic);
                await sender.SendMessageAsync(message);
                log.LogInformation("Message Sent Successfully");
            }
            catch (Exception e)
            {
                log.LogError("Send Message To Topic Error: " + e.StackTrace);
                throw;
            }
        }

        public string GetTopic(string touchPointId, ILogger log)
        {
            if (_activeTouchPoints != null && _activeTouchPoints.Contains(touchPointId))
            {
                return touchPointId;
            }

            log.LogWarning("Touchpoint {0} invalid, returning empty string", touchPointId);
            return String.Empty;
        }

    }
}
