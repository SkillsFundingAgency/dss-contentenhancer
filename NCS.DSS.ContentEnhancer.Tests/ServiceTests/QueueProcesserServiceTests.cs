using NCS.DSS.ContentEnhancer.Cosmos.Helper;
using Moq;
using NCS.DSS.ContentEnhancer.Service;
using NCS.DSS.ContentEnhancer.Models;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentEnhancer.Processor;
namespace NCS.DSS.ContentEnhancer.Tests.ServiceTests
{
    public class QueueProcesserServiceTests
    {
        private Mock<ISubscriptionHelper> _subscriptionHelper;
        private Mock<IMessageHelper> _messageHelper;
        private IQueueProcessorService _queueProcessorService;
        private Mock<ILogger<QueueProcessor>> _logger;
        private MessageModel _messageModel;
        private List<Subscriptions> _subscriptions;
        private const string customerId = "58b43e3f-4a50-4900-9c82-a14682ee90fa";
        private const string touchPointId = "9000000003";
        [SetUp]
        public void Setup()
        {
            _subscriptionHelper = new Mock<ISubscriptionHelper>();
            _messageHelper = new Mock<IMessageHelper>();
            _logger = new Mock<ILogger<QueueProcessor>>();
            _queueProcessorService = new QueueProcessorService(_subscriptionHelper.Object,_messageHelper.Object);
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

        [Test]
        public async Task QueueProcesserService_Subscriptions_SendToTopicSuccessfullyAsync()
        {
            _messageHelper.Setup(x => x.GetTopic(touchPointId, _logger.Object)).Returns(touchPointId);
            _subscriptionHelper.Setup(x => x.GetSubscriptionsAsync(It.IsAny<MessageModel>(), _logger.Object)).Returns(Task.FromResult(_subscriptions));
            _messageHelper.Setup(x => x.SendMessageToTopicAsync(touchPointId, _logger.Object, _messageModel));
            try
            {
                await _queueProcessorService.SendToTopicAsync(_messageModel, _logger.Object);

                //ASSERT
                
                _logger.Verify(x => x.Log(
                       LogLevel.Information,
                       It.IsAny<EventId>(),
                       It.Is<It.IsAnyType>((object v, Type _) =>
                       v.ToString().Contains(string.Format("Subscriptions - Send Message to Topic {0}", touchPointId))),
                       It.IsAny<Exception>(),
                       (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

                Assert.That(true);
            }
            catch
            {
                // ASSERT
                Assert.That(false);
            }
            
        }
        [Test]
        public async Task QueueProcesserService_DataCollections_SendToTopicSuccessfullyAsync()
        {
            _messageModel.DataCollections = true;
            _messageHelper.Setup(x => x.GetTopic(touchPointId, _logger.Object)).Returns(touchPointId);
            _subscriptionHelper.Setup(x => x.GetSubscriptionsAsync(It.IsAny<MessageModel>(), _logger.Object)).Returns(Task.FromResult(_subscriptions));
            _messageHelper.Setup(x => x.SendMessageToTopicAsync(touchPointId, _logger.Object, _messageModel));
            try
            {
                await _queueProcessorService.SendToTopicAsync(_messageModel, _logger.Object);

                //ASSERT

                _logger.Verify(x => x.Log(
                       LogLevel.Information,
                       It.IsAny<EventId>(),
                       It.Is<It.IsAnyType>((object v, Type _) =>
                       v.ToString().Contains(string.Format("Data Collections - Send Message to Topic {0}", touchPointId))),
                       It.IsAny<Exception>(),
                       (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

                Assert.That(true);
            }
            catch
            {
                // ASSERT
                Assert.That(false);
            }

        }
        [Test]
        public async Task QueueProcesserService_FailedToGetSubscriptions()
        {
            _messageModel.CustomerGuid = null;
            _messageModel.TouchpointId = null;
            try
            {
                await _queueProcessorService.SendToTopicAsync(_messageModel, _logger.Object);

                //ASSERT

                _logger.Verify(x => x.Log(
                       LogLevel.Error, 
                       It.IsAny<EventId>(),
                       It.Is<It.IsAnyType>((object v, Type _) =>
                       v.ToString().Contains("Failed to get subscriptions")),
                       It.IsAny<Exception>(),
                       (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

                Assert.That(true);
            }
            catch
            {
                // ASSERT
                Assert.That(false);
            }

        }
    }
}