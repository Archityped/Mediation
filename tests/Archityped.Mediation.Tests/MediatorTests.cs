using Archityped.Mediation.Tests.Mocks;


namespace Archityped.Mediation.Tests;

public class MediatorTests
{
    [Fact]
    public async Task SendAsync_WithResponse_ShouldReturnExpectedResponse()
    {
        // Arrange
        var mediator = CreateMediator(cfg => cfg.AddRequestHandler<MockRequestHandlerWithResponse>());
        var message = "Hello, World!";
        var request = new MockRequestWithResponse(message);

        // Act
        var response = await mediator.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(message, response);
    }

    [Fact]
    public async Task SendAsync_WithResponse_ShouldHandleCancellation()
    {
        // Arrange
        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler<MockRequestHandlerWithResponse>());

        var request = new MockRequestWithResponse("Hello, World!");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => mediator.SendAsync(request, cts.Token));
    }

    [Fact]
    public async Task SendAsync_WithoutResponse_ShouldCompleteSuccessfully()
    {
        // Arrange
        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler<MockRequestHandler>());

        var request = new MockRequest("Hello, World!");


        // Act & Assert (Should not throw)
        await mediator.SendAsync(request);
    }

    [Fact]
    public async Task SendAsync_WithoutResponse_ShouldHandleCancellation()
    {
        // Arrange
        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler<MockRequestHandler>());

        var request = new MockRequest("Hello, World!");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => mediator.SendAsync(request, cts.Token));
    }

    [Fact]
    public async Task PublishAsync_ShouldCallAllEventHandlers()
    {
        // Arrange
        var mediator = CreateMediator(cfg => cfg
            .AddEventHandler<MockEventHandler>());

        var testEvent = new MockEvent("Test event message");

        // Act & Assert (Should not throw)
        await mediator.PublishAsync(testEvent);
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleCancellation()
    {
        // Arrange
        var mediator = CreateMediator(cfg => cfg
            .AddEventHandler<MockEventHandler>());

        var testEvent = new MockEvent("Test event message");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => mediator.PublishAsync(testEvent, cts.Token));
    }

    [Fact]
    public async Task StreamAsync_ShouldReturnExpectedItems()
    {
        // Arrange
        var mediator = CreateMediator(cfg => cfg
            .AddStreamRequestHandler<MockStreamRequestHandler>());

        var request = new MockStreamRequest(3);
        var results = new List<int>();

        // Act
        await foreach (var item in mediator.StreamAsync(request))
        {
            results.Add(item);
        }

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal([0, 1, 2], results);
    }

    [Fact]
    public async Task StreamAsync_ShouldHandleCancellation()
    {
        // Arrange
        var mediator = CreateMediator(cfg => cfg
            .AddStreamRequestHandler<MockStreamRequestHandler>());

        var request = new MockStreamRequest(20, 25);
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            var asyncEnumerable = mediator.StreamAsync(request, cts.Token);
            await foreach (var _ in asyncEnumerable)
            {
                // This should be cancelled before completing
            }
        });
    }

    [Fact]
    public async Task StreamAsync_ShouldReturnEmptyForZeroCount()
    {
        // Arrange
        var mediator = CreateMediator(cfg => cfg
            .AddStreamRequestHandler<MockStreamRequestHandler>());

        var request = new MockStreamRequest(0);
        var results = new List<int>();

        // Act
        await foreach (var item in mediator.StreamAsync(request))
        {
            results.Add(item);
        }

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task SendAsync_WithMiddleware_ShouldInvokeMiddleware()
    {
        // Arrange
        var middleware = new MockRequestMiddleware();
        var mediator = CreateMediator(cfg => cfg
            .AddRequestHandler<MockRequestHandlerWithResponse>()
            .AddRequestMiddleware<MockRequestMiddleware>(_ => middleware));
        var message = "Hello World!";
        var request = new MockRequestWithResponse(message);

        // Act & Assert
        middleware.BeforeHandler = (req) =>
        {
            Assert.IsType<MockRequestWithResponse>(req);
            Assert.Equal(message, As<MockRequestWithResponse>(req).Message);
        };
        middleware.AfterHandler = (req, res) =>
        {
            Assert.IsType<MockRequestWithResponse>(req);
            Assert.Equal(message, As<MockRequestWithResponse>(req).Message);

            Assert.IsType<string>(res);
            Assert.Equal(message, res);
        };

        await mediator.SendAsync(request);
    }

    private static IMediator CreateMediator(Action<MediatorConfiguration>? configuration = null)
    {
        var serviceProvider = new ServiceCollection()
            .AddMediator(configuration)
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<IMediator>();
    }
}
