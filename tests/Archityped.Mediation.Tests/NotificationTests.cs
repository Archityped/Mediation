namespace Archityped.Mediation.Tests;

public record MockNotification(string Message) : INotification;

[Trait("Mediation", "INotification")]
public class NotificationTests
{
    [Fact]
    public async Task PublishAsync_HandlerShouldBeInvoked()
    {
        // Arrange
        var mockHandler = CreateMockNotificationHandler<MockNotification>();
        var mediator = CreateMediator(cfg => cfg.AddNotificationHandler(_ => mockHandler.Object));
        var testNotification = new MockNotification("Test notification message");

        // Act
        await mediator.PublishAsync(testNotification);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(It.IsAny<MockNotification>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task PublishAsync_InvokesAllHandlers()
    {
        // Arrange
        var h1 = CreateMockNotificationHandler<MockNotification>();
        var h2 = CreateMockNotificationHandler<MockNotification>();
        var testNotification = new MockNotification("Test notification message");
        var mediator = CreateMediator(cfg => cfg
            .AddNotificationHandler(_ => h1.Object)
            .AddNotificationHandler(_ => h2.Object));

        // Act
        await mediator.PublishAsync(testNotification);

        // Assert
        h1.Verify(h => h.HandleAsync(It.IsAny<MockNotification>(), It.IsAny<CancellationToken>()), Times.Once());
        h2.Verify(h => h.HandleAsync(It.IsAny<MockNotification>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task PublishAsync_HandlerShouldNotBeInvokedOnCancellation()
    {
        // Arrange
        var mockHandler = CreateMockNotificationHandler<MockNotification>();
        var mediator = CreateMediator(cfg => cfg.AddNotificationHandler(_ => mockHandler.Object));
        var testNotification = new MockNotification("Test notification message");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => mediator.PublishAsync(testNotification, cts.Token));

        // Assert
        mockHandler.Verify(h => h.HandleAsync(It.IsAny<MockNotification>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    /// <summary>
    /// Creates a mock INotificationHandler for the given notification type.
    /// </summary>
    private static Mock<INotificationHandler<TNotification>> CreateMockNotificationHandler<TNotification>()
        where TNotification : class, INotification
    {
        var mock = new Mock<INotificationHandler<TNotification>>();
        mock.Setup(h => h.HandleAsync(It.IsAny<TNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }
}
