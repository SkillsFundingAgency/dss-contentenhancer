using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.ContentEnhancer.Models;
using NCS.DSS.ContentEnhancer.Processor;
using NCS.DSS.ContentEnhancer.Services;
using NUnit.Framework.Internal;

namespace NCS.DSS.ContentEnhancer.Tests.FunctionTests
{
    public class QueueProcesserTests
    {
        private Mock<ISubscriptionService> _subscriptionService;
        private Mock<IMessagingService> _messagingService;
        private IQueueProcessor _queueProcessor;
        private Mock<ILogger<QueueProcessor>> _logger;
        private MessageModel _messageModel;
        private List<Subscriptions> _subscriptions;
        private const string customerId = "58b43e3f-4a50-4900-9c82-a14682ee90fa";
        private const string touchPointId = "9000000003";
        [SetUp]
        public void Setup()
        {
            _subscriptionService = new Mock<ISubscriptionService>();
            _messagingService = new Mock<IMessagingService>();
            _logger = new Mock<ILogger<QueueProcessor>>();
            _queueProcessor = new QueueProcessor(_logger.Object, _messagingService.Object, _subscriptionService.Object);
            _subscriptions = [new Subscriptions() { TouchPointId = touchPointId, CustomerId = Guid.Parse(customerId), SubscriptionId = Guid.NewGuid() }];
            _messageModel = new MessageModel()
            {
                TouchpointId = touchPointId,
                CustomerGuid = Guid.Parse(customerId),
                EmailAddress = "test@test.com",
                FirstName = "Test First name",
                LastName = "Test Last name"
            };
        }

        /*[Test]
        public void QueueProcesser_SendMessagesSuccessfully()
        {
            _messagingService.Setup(x => x.GetTopic(touchPointId, _logger.Object)).Returns(touchPointId);
            _subscriptionService.Setup(x => x.GetSubscriptionsAsync(It.IsAny<MessageModel>(), _logger.Object));
            try
            {
                _queueProcessor.RunAsync(_messageModel);

                //ASSERT

                _logger.Verify(x => x.Log(
                       LogLevel.Information, //Change LogLevel as required
                       It.IsAny<EventId>(),
                       It.Is<It.IsAnyType>((object v, Type _) =>
                       v.ToString().Contains("messages has been processed")), // Change MessageToVerify as required
                       It.IsAny<Exception>(),
                       (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

                Assert.That(true);
            }
            catch
            {
                // ASSERT
                Assert.That(false);
            }
        }*/
    }
}