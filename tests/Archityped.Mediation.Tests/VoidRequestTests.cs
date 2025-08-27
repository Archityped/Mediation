namespace Archityped.Mediation.Tests;

public record MockRequest(string Message) : IRequest;

[Trait("Mediation", "IRequest")]
public class VoidRequestTests
{
    [Fact]
    public async Task SendAsync_InvokesHandlerOnce()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<MockRequest>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()))
               .Returns<MockRequest, CancellationToken>((_, _) => Task.CompletedTask);

        var mediator = CreateMediator(cfg => cfg.AddRequestHandler(_ => handler.Object));

        // Act
        await mediator.SendAsync(new MockRequest("x"));

        // Assert
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task SendAsync_DoesNotInvokeHandlerWhenPreCanceled()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<MockRequest>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var mediator = CreateMediator(cfg => cfg.AddRequestHandler(_ => handler.Object));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act / Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => mediator.SendAsync(new MockRequest("x"), cts.Token));

        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task SendAsync_ThrowsWhenHandlerNotRegistered()
    {
        // Arrange
        var mediator = CreateMediator(_ => { });

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.SendAsync(new MockRequest("x")));

        // Assert
        Assert.Contains("handler", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Mediation", "IRequestMiddleware")]
    [Fact]
    public async Task SendAsync_InvokesSingleMiddleware()
    {
        // Arrange
        var middleware = new Mock<IRequestMiddleware>();
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequest>(),
                It.IsAny<RequestHandlerDelegate<VoidResult>>(),
                It.IsAny<CancellationToken>()))
            .Returns<MockRequest, RequestHandlerDelegate<VoidResult>, CancellationToken>((_, next, ct) => next(ct));

        var handler = new Mock<IRequestHandler<MockRequest>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware.Object));

        // Act
        await mediator.SendAsync(new MockRequest("hello"));

        // Assert
        middleware.Verify(m => m.InvokeAsync(
            It.IsAny<MockRequest>(),
            It.IsAny<RequestHandlerDelegate<VoidResult>>(),
            It.IsAny<CancellationToken>()), Times.Once());
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task SendAsync_InvokesMiddlewareInRegistrationOrder()
    {
        // Arrange
        var sequence = new MockSequence();

        var middleware1 = new Mock<IRequestMiddleware>();
        middleware1.InSequence(sequence)
           .Setup(m => m.InvokeAsync(
               It.IsAny<MockRequest>(),
               It.IsAny<RequestHandlerDelegate<VoidResult>>(),
               It.IsAny<CancellationToken>()))
           .Returns<MockRequest, RequestHandlerDelegate<VoidResult>, CancellationToken>((_, next, ct) => next(ct));

        var middleware2 = new Mock<IRequestMiddleware>();
        middleware2.InSequence(sequence)
           .Setup(m => m.InvokeAsync(
               It.IsAny<MockRequest>(),
               It.IsAny<RequestHandlerDelegate<VoidResult>>(),
               It.IsAny<CancellationToken>()))
           .Returns<MockRequest, RequestHandlerDelegate<VoidResult>, CancellationToken>((_, next, ct) => next(ct));

        var handler = new Mock<IRequestHandler<MockRequest>>();
        handler.InSequence(sequence)
               .Setup(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware1.Object)
            .AddRequestMiddleware(_ => middleware2.Object));

        // Act
        await mediator.SendAsync(new MockRequest("x"));

        // Assert
        middleware1.Verify(m => m.InvokeAsync(It.IsAny<MockRequest>(), It.IsAny<RequestHandlerDelegate<VoidResult>>(), It.IsAny<CancellationToken>()), Times.Once());
        middleware2.Verify(m => m.InvokeAsync(It.IsAny<MockRequest>(), It.IsAny<RequestHandlerDelegate<VoidResult>>(), It.IsAny<CancellationToken>()), Times.Once());
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task SendAsync_ShortCircuitsWhenMiddlewareDoesNotCallNext()
    {
        // Arrange
        var middleware = new Mock<IRequestMiddleware>();
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequest>(),
                It.IsAny<RequestHandlerDelegate<VoidResult>>(),
                It.IsAny<CancellationToken>()))
          .Returns<MockRequest, RequestHandlerDelegate<VoidResult>, CancellationToken>((_, _, _) => Task.FromResult(default(VoidResult)));

        var handler = new Mock<IRequestHandler<MockRequest>>();
        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware.Object));

        // Act
        await mediator.SendAsync(new MockRequest("ignored"));

        // Assert
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()), Times.Never());
        middleware.Verify(m => m.InvokeAsync(It.IsAny<MockRequest>(), It.IsAny<RequestHandlerDelegate<VoidResult>>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task SendAsync_PropagatesHandlerException()
    {
        // Arrange
        var middleware = new Mock<IRequestMiddleware>();
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequest>(),
                It.IsAny<RequestHandlerDelegate<VoidResult>>(),
                It.IsAny<CancellationToken>()))
          .Returns<MockRequest, RequestHandlerDelegate<VoidResult>, CancellationToken>((_, next, ct) => next(ct));

        var handler = new Mock<IRequestHandler<MockRequest>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("HANDLER"));

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware.Object));

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.SendAsync(new MockRequest("x")));

        middleware.Verify(m => m.InvokeAsync(It.IsAny<MockRequest>(), It.IsAny<RequestHandlerDelegate<VoidResult>>(), It.IsAny<CancellationToken>()), Times.Once());
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task SendAsync_StopsPipelineWhenFirstMiddlewareThrows()
    {
        // Arrange
        var middleware1 = new Mock<IRequestMiddleware>();
        middleware1.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequest>(),
                It.IsAny<RequestHandlerDelegate<VoidResult>>(),
                It.IsAny<CancellationToken>()))
           .ThrowsAsync(new InvalidOperationException("MIDDLEWARE1"));

        var middleware2 = new Mock<IRequestMiddleware>();
        var handler = new Mock<IRequestHandler<MockRequest>>();

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware1.Object)
            .AddRequestMiddleware(_ => middleware2.Object));

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.SendAsync(new MockRequest("x")));

        middleware2.Verify(m => m.InvokeAsync(It.IsAny<MockRequest>(), It.IsAny<RequestHandlerDelegate<VoidResult>>(), It.IsAny<CancellationToken>()), Times.Never());
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task SendAsync_InvokesPipelineOnEachCall()
    {
        // Arrange
        var callCount = 0;
        var middleware = new Mock<IRequestMiddleware>();
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequest>(),
                It.IsAny<RequestHandlerDelegate<VoidResult>>(),
                It.IsAny<CancellationToken>()))
          .Returns<MockRequest, RequestHandlerDelegate<VoidResult>, CancellationToken>(async (_, next, ct) =>
          {
              Interlocked.Increment(ref callCount);
              await next(ct);
              return default;
          });

        var handler = new Mock<IRequestHandler<MockRequest>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware.Object));

        // Act
        await mediator.SendAsync(new MockRequest("1"));
        await mediator.SendAsync(new MockRequest("2"));

        // Assert
        Assert.Equal(2, callCount);
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SendAsync_DoesNotInvokeHandlerWhenCanceledByMiddleware()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var mw = new Mock<IRequestMiddleware>();
        mw.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequest>(),
                It.IsAny<RequestHandlerDelegate<VoidResult>>(),
                It.IsAny<CancellationToken>()))
          .Returns<MockRequest, RequestHandlerDelegate<VoidResult>, CancellationToken>((_, next, ct) =>
          {
              cts.Cancel();
              ct.ThrowIfCancellationRequested();
              return next(ct);
          });

        var handler = new Mock<IRequestHandler<MockRequest>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => mw.Object));

        // Act / Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            mediator.SendAsync(new MockRequest("x"), cts.Token));

        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task SendAsync_PassesSameRequestInstanceThroughPipeline()
    {
        // Arrange
        var middleware = new Mock<IRequestMiddleware>();
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequest>(),
                It.IsAny<RequestHandlerDelegate<VoidResult>>(),
                It.IsAny<CancellationToken>()))
          .Returns<MockRequest, RequestHandlerDelegate<VoidResult>, CancellationToken>((req, next, ct) => next(ct));

        var handler = new Mock<IRequestHandler<MockRequest>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequest>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware.Object));

        var request = new MockRequest("x");

        // Act
        await mediator.SendAsync(request);

        // Assert
        middleware.Verify(m => m.InvokeAsync(
            It.Is<MockRequest>(r => ReferenceEquals(r, request)),
            It.IsAny<RequestHandlerDelegate<VoidResult>>(),
            It.IsAny<CancellationToken>()), Times.Once());
        handler.Verify(h => h.HandleAsync(
            It.Is<MockRequest>(r => ReferenceEquals(r, request)),
            It.IsAny<CancellationToken>()), Times.Once());
    }
}
