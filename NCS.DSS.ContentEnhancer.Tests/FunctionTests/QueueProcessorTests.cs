using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.ContentEnhancer.Models;
using NCS.DSS.ContentEnhancer.Processor;
using NCS.DSS.ContentEnhancer.Services;

namespace NCS.DSS.ContentEnhancer.Tests.FunctionTests
{
    public class QueueProcessorTests
    {
        private Mock<ISubscriptionService>? _subscriptionServiceMock;
        private Mock<IMessagingService>? _messagingServiceMock;
        private Mock<ILogger<QueueProcessor>>? _loggerMock;
        private IQueueProcessor? _queueProcessor;
        private MessageModel? _messageModel;
        private const string CustomerId = "58b43e3f-4a50-4900-9c82-a14682ee90fa";
        private const string TouchPointId = "9000000003";

        [SetUp]
        public void Setup()
        {
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            _messagingServiceMock = new Mock<IMessagingService>();
            _loggerMock = new Mock<ILogger<QueueProcessor>>();

            _queueProcessor = new QueueProcessor(_loggerMock.Object, _messagingServiceMock.Object, _subscriptionServiceMock.Object);

            _messageModel = new MessageModel
            {
                TouchpointId = TouchPointId,
                CustomerGuid = Guid.Parse(CustomerId),
                EmailAddress = "test@test.com",
                FirstName = "Test First name",
                LastName = "Test Last name"
            };
        }

        [Test]
        public async Task QueueProcessor_SendMessagesSuccessfully()
        {
            // Arrange
            _messagingServiceMock.Setup(x => x.GetTopic(TouchPointId, _loggerMock.Object)).Returns(TouchPointId);
            _subscriptionServiceMock.Setup(x => x.GetSubscriptionsAsync(It.IsAny<MessageModel>(), _loggerMock.Object)).ReturnsAsync(new List<Subscriptions>());

            // Act
            try
            {
                await _queueProcessor.RunAsync(_messageModel);

                // Assert
                _loggerMock.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("QueueProcessor has finished invoking")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected no exception, but got: {ex.Message}");
            }
        }

    }
}
