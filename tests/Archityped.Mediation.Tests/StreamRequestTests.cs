using System.Runtime.CompilerServices;

namespace Archityped.Mediation.Tests;

public record MockStreamRequest(int Count, int Delay = 0) : IStreamRequest<int>;

[Trait("Mediation", "IStreamRequest<T>")]
public class StreamRequestTests
{
    [Fact]
    public async Task StreamAsync_HandlerShouldBeInvoked()
    {
        // Arrange
        var mockHandler = CreateMockStreamRequestHandler<MockStreamRequest, int>((req, _) => GetStreamWithDelay(req.Count));
        var mediator = CreateMediator(cfg => cfg.AddStreamRequestHandler(_ => mockHandler.Object));
        var request = new MockStreamRequest(3);

        // Act
        await foreach (var _ in mediator.StreamAsync<MockStreamRequest, int>(request)) { }

        // Assert
        mockHandler.Verify(h => h.HandleAsync(It.IsAny<MockStreamRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task StreamAsync_HandlerShouldBeInvokedBeforeCancellation()
    {
        // Arrange
        var handler = CreateMockStreamRequestHandler<MockStreamRequest, int>((req, ct) => GetStreamWithDelay(req.Count, req.Delay, ct));
        var mediator = CreateMediator(cfg => cfg.AddStreamRequestHandler(_ => handler.Object));
        var request = new MockStreamRequest(20, 25);
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        // Act
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            var asyncEnumerable = mediator.StreamAsync<MockStreamRequest, int>(request, cts.Token);
            await foreach (var _ in asyncEnumerable) { }
        });

        // Assert
        handler.Verify(h => h.HandleAsync(It.IsAny<MockStreamRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task StreamAsync_HandlerShouldBeInvokedForZeroCount()
    {
        // Arrange
        var handler = CreateMockStreamRequestHandler<MockStreamRequest, int>((req, _) => GetStreamWithDelay(req.Count));
        var mediator = CreateMediator(cfg => cfg.AddStreamRequestHandler(_ => handler.Object));
        var request = new MockStreamRequest(0);

        // Act
        await foreach (var _ in mediator.StreamAsync<MockStreamRequest, int>(request)) { }

        // Assert
        handler.Verify(h => h.HandleAsync(It.IsAny<MockStreamRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Trait("Mediation", "IStreamRequestMiddleware")]
    [Fact]
    public async Task StreamAsync_WithStreamMiddleware_ShouldInvokeStreamMiddleware()
    {
        // Arrange
        var middleware = new Mock<IStreamRequestMiddleware>();
        var request = new MockStreamRequest(2);
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockStreamRequest>(),
                It.IsAny<StreamRequestHandlerDelegate<int>>(),
                It.IsAny<CancellationToken>()))
            .Returns<MockStreamRequest, StreamRequestHandlerDelegate<int>, CancellationToken>((req, next, ct) => next(ct));

        var mediator = CreateMediator(cfg => cfg
            .AddStreamRequestHandler(_ =>
            {
                var handler = new Mock<IStreamRequestHandler<MockStreamRequest, int>>();
                handler.Setup(h => h.HandleAsync(It.IsAny<MockStreamRequest>(), It.IsAny<CancellationToken>()))
                    .Returns(GetStreamWithDelay(2));
                return handler.Object;
            })
            .AddStreamRequestMiddleware(_ => middleware.Object));

        // Act
        var results = new List<int>();
        await foreach (var item in mediator.StreamAsync<MockStreamRequest, int>(request))
        {
            results.Add(item);
        }

        // Assert
        Assert.Equal([0, 1], results);
        middleware.Verify(m => m.InvokeAsync(
            It.IsAny<MockStreamRequest>(),
            It.IsAny<StreamRequestHandlerDelegate<int>>(),
            It.IsAny<CancellationToken>()), Times.Once());
    }

    /// <summary>
    /// Creates a mock IStreamRequestHandler for the given request/response types and setup function.
    /// </summary>
    private static Mock<IStreamRequestHandler<TRequest, TResponse>> CreateMockStreamRequestHandler<TRequest, TResponse>(Func<TRequest, CancellationToken, IAsyncEnumerable<TResponse>> responseFunc)
        where TRequest : class, IStreamRequest<TResponse>
    {
        var mock = new Mock<IStreamRequestHandler<TRequest, TResponse>>();
        mock.Setup(h => h.HandleAsync(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .Returns((TRequest req, CancellationToken ct) => responseFunc(req, ct));
        return mock;
    }

    /// <summary>
    /// Returns an async stream of integers with optional delay and cancellation support.
    /// </summary>
    private static async IAsyncEnumerable<int> GetStreamWithDelay(int count, int delay = 0, [EnumeratorCancellation] CancellationToken ct = default)
    {
        for (int i = 0; i < count; i++)
        {
            if (delay > 0)
                await Task.Delay(delay, ct);
            yield return i;
        }
    }
}
