using System.Runtime.CompilerServices;

namespace Archityped.Mediation.Tests;

public record StreamPreProcessorRequest : IStreamRequest<int>;

[Trait("Mediation", "IStreamRequestPreProcessor")]
public class StreamRequestPreProcessorsTests
{
    [Fact]
    public async Task StreamAsync_PreProcessorRunsBeforeHandler()
    {
        // Arrange
        var mockSequence = new MockSequence();

        var preProcessor = new Mock<IStreamRequestPreProcessor>();
        preProcessor.InSequence(mockSequence)
            .Setup(p => p.ProcessAsync<StreamPreProcessorRequest, int>(
                It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var handler = new Mock<IStreamRequestHandler<StreamPreProcessorRequest, int>>();
        handler.InSequence(mockSequence)
            .Setup(h => h.HandleAsync(It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()))
            .Returns(GetStream());

        var mediator = CreateMediator(cfg => cfg
            .AddStreamRequestHandler(_ => handler.Object)
            .AddStreamRequestProcessor(_ => preProcessor.Object));

        // Act
        await foreach (var _ in mediator.StreamAsync<StreamPreProcessorRequest, int>(new())) { }

        // Assert
        preProcessor.Verify(p => p.ProcessAsync<StreamPreProcessorRequest, int>(It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        handler.Verify(h => h.HandleAsync(It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task StreamAsync_MultiplePreProcessorsRunInRegistrationOrder()
    {
        // Arrange
        var mockSequence = new MockSequence();

        var preProcessor1 = new Mock<IStreamRequestPreProcessor>();
        preProcessor1.InSequence(mockSequence)
            .Setup(p => p.ProcessAsync<StreamPreProcessorRequest, int>(
                It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var preProcessor2 = new Mock<IStreamRequestPreProcessor>();
        preProcessor2.InSequence(mockSequence)
            .Setup(p => p.ProcessAsync<StreamPreProcessorRequest, int>(
                It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var handler = new Mock<IStreamRequestHandler<StreamPreProcessorRequest, int>>();
        handler.InSequence(mockSequence)
            .Setup(h => h.HandleAsync(It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()))
            .Returns(GetStream());

        var mediator = CreateMediator(cfg => cfg
            .AddStreamRequestHandler(_ => handler.Object)
            .AddStreamRequestProcessor(_ => preProcessor1.Object)
            .AddStreamRequestProcessor(_ => preProcessor2.Object));

        // Act
        await foreach (var _ in mediator.StreamAsync<StreamPreProcessorRequest, int>(new())) { }

        // Assert
        preProcessor1.Verify(p => p.ProcessAsync<StreamPreProcessorRequest, int>(It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        preProcessor2.Verify(p => p.ProcessAsync<StreamPreProcessorRequest, int>(It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        handler.Verify(h => h.HandleAsync(It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task StreamAsync_ExceptionInFirstPreProcessorPreventsSubsequentPreProcessorsAndHandler()
    {
        // Arrange
        var preProcessor1 = new Mock<IStreamRequestPreProcessor>();
        preProcessor1
            .Setup(p => p.ProcessAsync<StreamPreProcessorRequest, int>(
                It.IsAny<StreamPreProcessorRequest>(),
                It.IsAny<CancellationToken>()))
            .Throws(new InvalidOperationException("Boom!"));

        var preProcessor2 = new Mock<IStreamRequestPreProcessor>();
        var handler = new Mock<IStreamRequestHandler<StreamPreProcessorRequest, int>>();
        handler
            .Setup(h => h.HandleAsync(It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()))
            .Returns(GetStream());

        var mediator = CreateMediator(cfg => cfg
            .AddStreamRequestHandler(_ => handler.Object)
            .AddStreamRequestProcessor(_ => preProcessor1.Object)
            .AddStreamRequestProcessor(_ => preProcessor2.Object));

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in mediator.StreamAsync<StreamPreProcessorRequest, int>(new())) { }
        });

        preProcessor2.Verify(p => p.ProcessAsync<StreamPreProcessorRequest, int>(It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        handler.Verify(h => h.HandleAsync(It.IsAny<StreamPreProcessorRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static async IAsyncEnumerable<int> GetStream(int iterations = 3, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var index in Enumerable.Range(0, iterations))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return index;
            await Task.Yield();
        }
    }
}