using Archityped.Mediation.Benchmarks.Mocks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Archityped.Mediation.Benchmarks;

public class Benchmarks
{
    // Archityped mediators
    private IMediator mediator;
    private IMediator mediatorWithPipeline;

    // Archityped message instances
    private readonly VoidRequest request = new();
    private readonly Request request_1 = new();
    private readonly StreamRequest streamRequest_1 = new(Items: 8);
    private readonly Notification notification = new();

    [GlobalSetup]
    public void Setup()
    {
        // Archityped baseline
        var architypedServices = new ServiceCollection();
        architypedServices.AddMediator(cfg =>
        {
            cfg.AddRequestHandler<RequestHandler>()
               .AddRequestHandler<VoidRequestHandler>()
               .AddStreamRequestHandler<StreamRequestHandler>()
               .AddNotificationHandler<NotificationHandler>();
        });

        mediator = architypedServices.BuildServiceProvider().GetRequiredService<IMediator>();

        // Archityped with middleware
        var architypedServicesWithMiddleware = new ServiceCollection();
        architypedServicesWithMiddleware.AddMediator(cfg =>
        {
            cfg.AddRequestHandler<RequestHandler>()
               .AddRequestHandler<VoidRequestHandler>()
               .AddStreamRequestHandler<StreamRequestHandler>()
               .AddNotificationHandler<NotificationHandler>()
               .AddRequestMiddleware<RequestMiddleware>()
               .AddStreamRequestMiddleware<StreamRequestMiddleware>();
        });

        mediatorWithPipeline = architypedServicesWithMiddleware.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Benchmark(Description = "Archityped.IRequest")]
    public Task Archityped_Request_Void() =>
        mediator.SendAsync(request);

    [Benchmark(Description = "Archityped.IRequest (with middleware pipeline)")]
    public Task Archityped_Request_Void_WithMiddleware() =>
        mediatorWithPipeline.SendAsync(request);

    [Benchmark(Description = "Archityped.IRequest<T>")]
    public Task Archityped_Request_Typed() =>
        mediator.SendAsync(request_1);

    [Benchmark(Description = "Archityped.IRequest<T> (with middleware pipeline)")]
    public Task Archityped_Request_Typed_WithMiddleware() =>
        mediatorWithPipeline.SendAsync(request_1);

    [Benchmark(Description = "Archityped.IStreamRequest<T>")]
    public async Task Archityped_StreamRequest()
    {
        var streamAccumulator = 0;
        await foreach (var item in mediator.StreamAsync(streamRequest_1))
            streamAccumulator += item;
    }

    [Benchmark(Description = "Archityped.IStreamRequest<T> (with middleware pipeline)")]
    public async Task Archityped_StreamRequest_WithMiddleware()
    {
        var streamAccumulator = 0;
        await foreach (var item in mediatorWithPipeline.StreamAsync(streamRequest_1))
            streamAccumulator += item;
    }

    [Benchmark(Description = "Archityped.INotification")]
    public Task Archityped_Notification() =>
        mediator.PublishAsync(notification);
}
