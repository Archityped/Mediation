namespace Archityped.Mediation.Tests;

public record RequestPreProcessorRequest : IRequest<string>;

[Trait("Mediation", "IRequestPreProcessor")]
public class RequestPreProcessorsTests
{
    [Fact]
    public async Task SendAsync_PreProcessorRunsBeforeHandler()
    {
        // Arrange
        var mockSequence = new MockSequence();

        var preProcessor = new Mock<IRequestPreProcessor>();
        preProcessor.InSequence(mockSequence)
            .Setup(p => p.ProcessAsync<RequestPreProcessorRequest, string>(
                It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var request = new Mock<IRequestHandler<RequestPreProcessorRequest, string>>();
        request.InSequence(mockSequence)
            .Setup(h => h.HandleAsync(
                It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("OK");

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => request.Object)
            .AddRequestProcessor(_ => preProcessor.Object));

        // Act
        var result = await mediator.SendAsync<RequestPreProcessorRequest, string>(new());

        // Assert
        preProcessor.Verify(p => p.ProcessAsync<RequestPreProcessorRequest, string>(It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        request.Verify(h => h.HandleAsync(It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_MultiplePreProcessorsRunInRegistrationOrder()
    {
        // Arrange
        var mockSequence = new MockSequence();

        var preProcessor1 = new Mock<IRequestPreProcessor>();
        preProcessor1.InSequence(mockSequence)
            .Setup(p => p.ProcessAsync<RequestPreProcessorRequest, string>(
                It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var preProcessor2 = new Mock<IRequestPreProcessor>();
        preProcessor2.InSequence(mockSequence)
            .Setup(p => p.ProcessAsync<RequestPreProcessorRequest, string>(
                It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var request = new Mock<IRequestHandler<RequestPreProcessorRequest, string>>();
        request.InSequence(mockSequence)
            .Setup(h => h.HandleAsync(
                It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("OK");

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => request.Object)
            .AddRequestProcessor(_ => preProcessor1.Object)
            .AddRequestProcessor(_ => preProcessor2.Object));

        // Act
        await mediator.SendAsync<RequestPreProcessorRequest, string>(new());

        // Assert
        preProcessor1.Verify(p => p.ProcessAsync<RequestPreProcessorRequest, string>(It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        preProcessor2.Verify(p => p.ProcessAsync<RequestPreProcessorRequest, string>(It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        request.Verify(h => h.HandleAsync(It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_ExceptionInFirstPreProcessorPreventsSubsequentPreProcessorsAndHandler()
    {
        // Arrange
        var mockSequence = new MockSequence();

        var preProcessor1 = new Mock<IRequestPreProcessor>();
        preProcessor1.InSequence(mockSequence)
             .Setup(p => p.ProcessAsync<RequestPreProcessorRequest, string>(
                 It.IsAny<RequestPreProcessorRequest>(),
                 It.IsAny<CancellationToken>()))
             .Throws(new InvalidOperationException("Poof!"));

        var preProcessor2 = new Mock<IRequestPreProcessor>();

        var request = new Mock<IRequestHandler<RequestPreProcessorRequest, string>>();
        request.InSequence(mockSequence)
             .Setup(h => h.HandleAsync(
                   It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync("OK");

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => request.Object)
            .AddRequestProcessor(_ => preProcessor1.Object)
            .AddRequestProcessor(_ => preProcessor2.Object));

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.SendAsync<RequestPreProcessorRequest, string>(new()));

        preProcessor2.Verify(p => p.ProcessAsync<RequestPreProcessorRequest, string>(It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Never());
        request.Verify(h => h.HandleAsync(It.IsAny<RequestPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
