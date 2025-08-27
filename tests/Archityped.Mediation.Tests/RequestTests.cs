namespace Archityped.Mediation.Tests;

public record MockRequestWithResponse(string Message) : IRequest<string>;

[Trait("Mediation", "IRequest<T>")]
public class RequestTests
{
    [Fact]
    public async Task SendAsync_ReturnsExpectedResponse()
    {
        // Arrange
        var message = "Hello, World!";
        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((MockRequestWithResponse r, CancellationToken _) => r.Message);
        var mediator = CreateMediator(cfg => cfg.AddRequestHandler(_ => handler.Object));

        // Act
        var response = await mediator.SendAsync(new MockRequestWithResponse(message));

        // Assert
        Assert.NotNull(response);
        Assert.Equal(message, response);
    }

    [Fact]
    public async Task SendAsync_InvokesHandlerOnce()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((MockRequestWithResponse r, CancellationToken _) => r.Message);
        var mediator = CreateMediator(cfg => cfg.AddRequestHandler(_ => handler.Object));

        // Act
        await mediator.SendAsync<MockRequestWithResponse, string>(new("x"));

        // Assert
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task SendAsync_DoesNotInvokeHandlerWhenPreCanceled()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(string.Empty);
        var mediator = CreateMediator(cfg => cfg.AddRequestHandler(_ => handler.Object));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act / Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => mediator.SendAsync<MockRequestWithResponse, string>(new("x"), cts.Token));

        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task SendAsync_ThrowsWhenHandlerNotRegistered()
    {
        // Arrange
        var mediator = CreateMediator(_ => { });

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.SendAsync<MockRequestWithResponse, string>(new("x")));

        // Assert
        Assert.Contains("handler", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Mediation", "IRequestMiddleware")]
    [Fact]
    public async Task SendAsync_InvokesSingleMiddleware()
    {
        // Arrange
        var middleware = new Mock<IRequestMiddleware>();
        var message = "Hello World!";
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequestWithResponse>(),
                It.IsAny<RequestHandlerDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<MockRequestWithResponse, RequestHandlerDelegate<string>, CancellationToken>((_, next, ct) => next(ct));

        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(message);

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware.Object));

        // Act
        var response = await mediator.SendAsync<MockRequestWithResponse, string>(new(message));

        // Assert
        Assert.Equal(message, response);
        middleware.Verify(m => m.InvokeAsync(
            It.IsAny<MockRequestWithResponse>(),
            It.IsAny<RequestHandlerDelegate<string>>(),
            It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task SendAsync_InvokesMiddlewareInRegistrationOrder()
    {
        // Arrange
        var sequence = new MockSequence();

        var middleware1 = new Mock<IRequestMiddleware>();
        middleware1.InSequence(sequence)
           .Setup(m => m.InvokeAsync(
               It.IsAny<MockRequestWithResponse>(),
               It.IsAny<RequestHandlerDelegate<string>>(),
               It.IsAny<CancellationToken>()))
           .Returns<MockRequestWithResponse, RequestHandlerDelegate<string>, CancellationToken>((_, next, ct) => next(ct));

        var middleware2 = new Mock<IRequestMiddleware>();
        middleware2.InSequence(sequence)
           .Setup(m => m.InvokeAsync(
               It.IsAny<MockRequestWithResponse>(),
               It.IsAny<RequestHandlerDelegate<string>>(),
               It.IsAny<CancellationToken>()))
           .Returns<MockRequestWithResponse, RequestHandlerDelegate<string>, CancellationToken>((_, next, ct) => next(ct));

        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        handler.InSequence(sequence)
               .Setup(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("OK");

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware1.Object)
            .AddRequestMiddleware(_ => middleware2.Object));

        // Act
        var result = await mediator.SendAsync<MockRequestWithResponse, string>(new("x"));

        // Assert
        Assert.Equal("OK", result);
        middleware1.Verify(m => m.InvokeAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<RequestHandlerDelegate<string>>(), It.IsAny<CancellationToken>()), Times.Once());
        middleware2.Verify(m => m.InvokeAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<RequestHandlerDelegate<string>>(), It.IsAny<CancellationToken>()), Times.Once());
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task SendAsync_ShortCircuitsWhenMiddlewareReturnsResult()
    {
        // Arrange
        var middleware = new Mock<IRequestMiddleware>();
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequestWithResponse>(),
                It.IsAny<RequestHandlerDelegate<string>>(),
                It.IsAny<CancellationToken>()))
          .Returns<MockRequestWithResponse, RequestHandlerDelegate<string>, CancellationToken>((_, _, _) => Task.FromResult("SHORTCIRCUIT"));

        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware.Object));

        // Act
        var result = await mediator.SendAsync<MockRequestWithResponse, string>(new("ignored"));

        // Assert
        Assert.Equal("SHORTCIRCUIT", result);
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task SendAsync_PropagatesHandlerException()
    {
        // Arrange
        var middleware = new Mock<IRequestMiddleware>();
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequestWithResponse>(),
                It.IsAny<RequestHandlerDelegate<string>>(),
                It.IsAny<CancellationToken>()))
          .Returns<MockRequestWithResponse, RequestHandlerDelegate<string>, CancellationToken>((_, next, ct) => next(ct));

        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("HANDLER"));

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware.Object));

        // Act / Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.SendAsync<MockRequestWithResponse, string>(new("x")));

        // Assert
        middleware.Verify(m => m.InvokeAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<RequestHandlerDelegate<string>>(), It.IsAny<CancellationToken>()), Times.Once());
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task SendAsync_StopsPipelineWhenFirstMiddlewareThrows()
    {
        // Arrange
        var middleware1 = new Mock<IRequestMiddleware>();
        middleware1.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequestWithResponse>(),
                It.IsAny<RequestHandlerDelegate<string>>(),
                It.IsAny<CancellationToken>()))
           .ThrowsAsync(new InvalidOperationException("MIDDLEWARE1"));

        var middleware2 = new Mock<IRequestMiddleware>();
        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware1.Object)
            .AddRequestMiddleware(_ => middleware2.Object));

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.SendAsync<MockRequestWithResponse, string>(new("x")));

        // Assert
        middleware2.Verify(m => m.InvokeAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<RequestHandlerDelegate<string>>(), It.IsAny<CancellationToken>()), Times.Never());
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task SendAsync_AppliesSingleMiddlewareTransformation()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("FOO");

        var middleware = new Mock<IRequestMiddleware>();
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequestWithResponse>(),
                It.IsAny<RequestHandlerDelegate<string>>(),
                It.IsAny<CancellationToken>()))
          .Returns<MockRequestWithResponse, RequestHandlerDelegate<string>, CancellationToken>(async (_, next, ct) =>
          {
              var inner = await next(ct);
              return inner + "BAR";
          });

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware.Object));

        // Act
        var result = await mediator.SendAsync<MockRequestWithResponse, string>(new("x"));

        // Assert
        Assert.Equal("FOOBAR", result);
    }

    [Fact]
    public async Task SendAsync_ComposesMiddlewareTransformations()
    {
        // Arrange
        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("HANDLER");

        var middleware1 = new Mock<IRequestMiddleware>();
        middleware1.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequestWithResponse>(),
                It.IsAny<RequestHandlerDelegate<string>>(),
                It.IsAny<CancellationToken>()))
           .Returns<MockRequestWithResponse, RequestHandlerDelegate<string>, CancellationToken>(async (_, next, ct) =>
           {
               var r = await next(ct);
               return r + "_MW1";
           });

        var middleware2 = new Mock<IRequestMiddleware>();
        middleware2.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequestWithResponse>(),
                It.IsAny<RequestHandlerDelegate<string>>(),
                It.IsAny<CancellationToken>()))
           .Returns<MockRequestWithResponse, RequestHandlerDelegate<string>, CancellationToken>(async (_, next, ct) =>
           {
               var r = await next(ct);
               return r + "_MW2";
           });

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware1.Object) // outer
            .AddRequestMiddleware(_ => middleware2.Object)); // inner

        // Act
        var result = await mediator.SendAsync<MockRequestWithResponse, string>(new("x"));

        // Assert
        Assert.Equal("HANDLER_MW2_MW1", result);
    }

    [Fact]
    public async Task SendAsync_InvokesPipelineOnEachCall()
    {
        // Arrange
        var callCount = 0;
        var middleware = new Mock<IRequestMiddleware>();
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequestWithResponse>(),
                It.IsAny<RequestHandlerDelegate<string>>(),
                It.IsAny<CancellationToken>()))
          .Returns<MockRequestWithResponse, RequestHandlerDelegate<string>, CancellationToken>(async (_, next, ct) =>
          {
              Interlocked.Increment(ref callCount);
              return await next(ct);
          });

        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("OK");

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware.Object));

        // Act
        await mediator.SendAsync<MockRequestWithResponse, string>(new("1"));
        await mediator.SendAsync<MockRequestWithResponse, string>(new("2"));

        // Assert
        Assert.Equal(2, callCount);
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SendAsync_DoesNotInvokeHandlerWhenCanceledByMiddleware()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var middleware = new Mock<IRequestMiddleware>();
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequestWithResponse>(),
                It.IsAny<RequestHandlerDelegate<string>>(),
                It.IsAny<CancellationToken>()))
          .Returns<MockRequestWithResponse, RequestHandlerDelegate<string>, CancellationToken>((_, next, ct) =>
          {
              cts.Cancel();
              ct.ThrowIfCancellationRequested();
              return next(ct);
          });

        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("UNREACHABLE");

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware.Object));

        // Act / Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            mediator.SendAsync<MockRequestWithResponse, string>(new("x"), cts.Token));

        // Assert (handler not invoked)
        handler.Verify(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task SendAsync_PassesSameRequestInstanceThroughPipeline()
    {
        // Arrange
        var middleware = new Mock<IRequestMiddleware>();
        middleware.Setup(m => m.InvokeAsync(
                It.IsAny<MockRequestWithResponse>(),
                It.IsAny<RequestHandlerDelegate<string>>(),
                It.IsAny<CancellationToken>()))
          .Returns<MockRequestWithResponse, RequestHandlerDelegate<string>, CancellationToken>((req, next, ct) => next(ct));

        var handler = new Mock<IRequestHandler<MockRequestWithResponse, string>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<MockRequestWithResponse>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("OK");

        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler(_ => handler.Object)
            .AddRequestMiddleware(_ => middleware.Object));

        var request = new MockRequestWithResponse("x");

        // Act
        await mediator.SendAsync<MockRequestWithResponse, string>(request);

        // Assert
        middleware.Verify(m => m.InvokeAsync(
            It.Is<MockRequestWithResponse>(r => ReferenceEquals(r, request)),
            It.IsAny<RequestHandlerDelegate<string>>(),
            It.IsAny<CancellationToken>()), Times.Once());
        handler.Verify(h => h.HandleAsync(
            It.Is<MockRequestWithResponse>(r => ReferenceEquals(r, request)),
            It.IsAny<CancellationToken>()), Times.Once());
    }
}
