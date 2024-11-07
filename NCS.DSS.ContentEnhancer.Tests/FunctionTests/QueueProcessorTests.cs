using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.ContentEnhancer.Models;
using NCS.DSS.ContentEnhancer.Processor;
using NCS.DSS.ContentEnhancer.Services;

namespace NCS.DSS.ContentEnhancer.Tests.FunctionTests
{
    public class QueueProcessorTests
    {
        private Mock<ISubscriptionService> _subscriptionServiceMock;
        private Mock<IMessagingService> _messagingServiceMock;
        private Mock<ILogger<QueueProcessor>> _loggerMock;
        private IQueueProcessor _queueProcessor;
        private MessageModel _messageModel;
        
        private const string CustomerId = "58b43e3f-4a50-4900-9c82-a14682ee90fa";
        private const string TouchpointId9001 = "9000000001";
        private const string TouchpointId9002 = "9000000002";
        private const string TouchpointId9003 = "9000000003";
        private const string TouchpointIdInvalid = "0000000000";

        [SetUp]
        public void Setup()
        {
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            _messagingServiceMock = new Mock<IMessagingService>();
            _loggerMock = new Mock<ILogger<QueueProcessor>>();

            _queueProcessor = new QueueProcessor(_loggerMock.Object, _messagingServiceMock.Object, _subscriptionServiceMock.Object);

            _messageModel = new MessageModel
            {
                TouchpointId = TouchpointId9003,
                CustomerGuid = Guid.Parse(CustomerId),
                EmailAddress = "test@test.com",
                FirstName = "Test First name",
                LastName = "Test Last name"
            };
        }

        [Test]
        public async Task InvalidMessageObject_ReturnsArgumentNullException()
        {
            // Arrange
            _messageModel = null!;

            // Act
            try
            {
                await _queueProcessor.RunAsync(_messageModel);
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ArgumentNullException))
                {
                    Assert.Fail($"Exception threw of type {ex.GetType()}");
                }
            }
        }
        
        [Test]
        public async Task DataCollectionsObjectWithoutTopic_ReturnsArgumentException()
        {
            // Arrange
            _messageModel.DataCollections = true;
            _messagingServiceMock.Setup(x => x.GetTopic(TouchpointId9003, _loggerMock.Object)).Returns(string.Empty);

            // Act
            try
            {
                await _queueProcessor.RunAsync(_messageModel);
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ArgumentException))
                {
                    Assert.Fail($"Exception threw of type {ex.GetType()}");
                }
            }
        }

        [Test]
        public async Task DataCollectionsObjectWithTopic_LogsExpectedInformation()
        {
            // Arrange
            _messageModel.DataCollections = true;
            _messagingServiceMock.Setup(x => x.GetTopic(TouchpointId9003, _loggerMock.Object)).Returns(TouchpointId9003);

            // Act
            await _queueProcessor.RunAsync(_messageModel);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Data Collections related message has been received"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        }

        [Test]
        public async Task CosmosDbRetrievalFailure_LogsExpectedError()
        {
            // Arrange
            _messagingServiceMock.Setup(x => x.GetTopic(TouchpointId9003, _loggerMock.Object)).Returns(TouchpointId9003);
            _subscriptionServiceMock.Setup(x => x.GetSubscriptionsAsync(It.IsAny<MessageModel>(), _loggerMock.Object)).Throws(new Exception());

            // Act
            try
            {
                await _queueProcessor.RunAsync(_messageModel);
            }
            catch (Exception ex)
            {
                // Assert
                _loggerMock.Verify(x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString()!.Contains("Failed to retrieve SUBSCRIPTIONS for Customer")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ), Times.Once);
            }
        }

        [Test]
        public async Task CustomerWithDigitalIdentity_LogsExpectedInformation()
        {
            _messageModel.IsDigitalAccount = true;

            // Arrange
            _messagingServiceMock.Setup(x => x.GetTopic(TouchpointId9003, _loggerMock.Object)).Returns(TouchpointId9003);
            _subscriptionServiceMock.Setup(x => x.GetSubscriptionsAsync(It.IsAny<MessageModel>(), _loggerMock.Object)).ReturnsAsync(new List<Subscriptions>{
                new Subscriptions() { CustomerId = Guid.Parse(CustomerId), Subscribe = true, SubscriptionId = new Guid(), TouchPointId = TouchpointId9001, LastModifiedBy = TouchpointId9001, LastModifiedDate = DateTime.Now }
            });

            // Act
            await _queueProcessor.RunAsync(_messageModel);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Digital Identity related message has been received"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        }

        [Test]
        public async Task CustomerWithASingleSubscription_LogsExpectedWarning()
        {
            // Arrange
            _messagingServiceMock.Setup(x => x.GetTopic(TouchpointId9003, _loggerMock.Object)).Returns(TouchpointId9003);
            _subscriptionServiceMock.Setup(x => x.GetSubscriptionsAsync(It.IsAny<MessageModel>(), _loggerMock.Object)).ReturnsAsync(new List<Subscriptions>());

            // Act
            await _queueProcessor.RunAsync(_messageModel);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Customer with GUID 58b43e3f-4a50-4900-9c82-a14682ee90fa does not have subscriptions associated with other touchpoint IDs. Originating touchpoint ID: 9000000003"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        }

        [Test]
        public async Task CustomerWithMultipleSubscriptions_LogsExpectedInformation()
        {
            // Arrange
            _messagingServiceMock.Setup(x => x.GetTopic(TouchpointId9003, _loggerMock.Object)).Returns(TouchpointId9003);
            _subscriptionServiceMock.Setup(x => x.GetSubscriptionsAsync(It.IsAny<MessageModel>(), _loggerMock.Object)).ReturnsAsync(new List<Subscriptions>(){
                new Subscriptions(){ CustomerId = Guid.Parse(CustomerId), Subscribe = true, SubscriptionId = new Guid(), TouchPointId = TouchpointId9001, LastModifiedBy = TouchpointId9001, LastModifiedDate = DateTime.Now },
                new Subscriptions(){ CustomerId = Guid.Parse(CustomerId), Subscribe = true, SubscriptionId = new Guid(), TouchPointId = TouchpointId9002, LastModifiedBy = TouchpointId9002, LastModifiedDate = DateTime.Now }
            });

            // Act
            await _queueProcessor.RunAsync(_messageModel);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Change notification related messages have been received - subscribers will now be notified"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        }
    }
}
