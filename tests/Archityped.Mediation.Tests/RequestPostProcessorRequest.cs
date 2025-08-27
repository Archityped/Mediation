namespace Archityped.Mediation.Tests;

public record RequestPostProcessorRequest : IRequest<string>;

[Trait("Mediation", "IRequestPostProcessor")]
public class RequestPostProcessorTests
{
    [Fact]
    public async Task SendAsync_PostProcessorRunsAfterHandler()
    {
        // Arrange
        var mockSequence = new MockSequence();

        var request = new Mock<IRequestHandler<RequestPostProcessorRequest, string>>();
        request.InSequence(mockSequence)
               .Setup(h => h.HandleAsync(
                   It.IsAny<RequestPostProcessorRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("x");

        var postProcessor = new Mock<IRequestPostProcessor>();
        postProcessor.InSequence(mockSequence)
            .Setup(p => p.ProcessAsync(
                It.IsAny<RequestPostProcessorRequest>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => request.Object)
            .AddRequestProcessor(_ => postProcessor.Object));

        // Act
        var result = await mediator.SendAsync<RequestPostProcessorRequest, string>(new());

        // Assert
        request.Verify(h => h.HandleAsync(
            It.IsAny<RequestPostProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        postProcessor.Verify(p => p.ProcessAsync(
            It.IsAny<RequestPostProcessorRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_MultiplePostProcessorsRunInRegistrationOrder()
    {
        // Arrange
        var mockSequence = new MockSequence();

        var request = new Mock<IRequestHandler<RequestPostProcessorRequest, string>>();
        request.InSequence(mockSequence)
               .Setup(h => h.HandleAsync(
                   It.IsAny<RequestPostProcessorRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("x");

        var postProcessor1 = new Mock<IRequestPostProcessor>();
        postProcessor1.InSequence(mockSequence)
             .Setup(p => p.ProcessAsync(
                 It.IsAny<RequestPostProcessorRequest>(),
                 It.IsAny<string>(),
                 It.IsAny<CancellationToken>()))
             .Returns(ValueTask.CompletedTask);

        var postProcessor2 = new Mock<IRequestPostProcessor>();
        postProcessor2.InSequence(mockSequence)
             .Setup(p => p.ProcessAsync(
                 It.IsAny<RequestPostProcessorRequest>(),
                 It.IsAny<string>(),
                 It.IsAny<CancellationToken>()))
             .Returns(ValueTask.CompletedTask);

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => request.Object)
            .AddRequestProcessor(_ => postProcessor1.Object)
            .AddRequestProcessor(_ => postProcessor2.Object));

        // Act
        await mediator.SendAsync<RequestPostProcessorRequest, string>(new());

        // Assert
        request.Verify(h => h.HandleAsync(
            It.IsAny<RequestPostProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        postProcessor1.Verify(p => p.ProcessAsync(
            It.IsAny<RequestPostProcessorRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        postProcessor2.Verify(p => p.ProcessAsync(
            It.IsAny<RequestPostProcessorRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_PostProcessorReceivesResponse()
    {
        // Arrange
        var expected = "RESULT";
        var mockSequence = new MockSequence();

        var request = new Mock<IRequestHandler<RequestPostProcessorRequest, string>>();
        request.InSequence(mockSequence)
               .Setup(h => h.HandleAsync(
                   It.IsAny<RequestPostProcessorRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(expected);

        var postProcessor = new Mock<IRequestPostProcessor>();
        postProcessor.InSequence(mockSequence)
            .Setup(p => p.ProcessAsync(
                It.IsAny<RequestPostProcessorRequest>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => request.Object)
            .AddRequestProcessor(_ => postProcessor.Object));

        // Act
        var result = await mediator.SendAsync<RequestPostProcessorRequest, string>(new());

        // Assert
        request.Verify(h => h.HandleAsync(
            It.IsAny<RequestPostProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        postProcessor.Verify(p => p.ProcessAsync(
            It.IsAny<RequestPostProcessorRequest>(), expected, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_ExceptionInFirstPostProcessorPreventsSubsequent()
    {
        // Arrange
        var mockSequence = new MockSequence();

        var request = new Mock<IRequestHandler<RequestPostProcessorRequest, string>>();
        request.InSequence(mockSequence)
               .Setup(h => h.HandleAsync(
                   It.IsAny<RequestPostProcessorRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("OK");

        var postProcessor1 = new Mock<IRequestPostProcessor>();
        postProcessor1.InSequence(mockSequence)
             .Setup(p => p.ProcessAsync(
                 It.IsAny<RequestPostProcessorRequest>(),
                 It.IsAny<string>(),
                 It.IsAny<CancellationToken>()))
             .Throws(new InvalidOperationException("Poof!"));

        var postProcessor2 = new Mock<IRequestPostProcessor>();

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => request.Object)
            .AddRequestProcessor(_ => postProcessor1.Object)
            .AddRequestProcessor(_ => postProcessor2.Object));

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.SendAsync<RequestPostProcessorRequest, string>(new()));

        postProcessor2.Verify(p => p.ProcessAsync(
            It.IsAny<RequestPostProcessorRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
