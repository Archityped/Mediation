using System.Runtime.CompilerServices;

namespace Archityped.Mediation;

/// <summary>
/// Represents an abstract base implementation of a mediator that provides core functionality for request handling, event publishing, and middleware processing.
/// </summary>
public abstract class BaseMediator : IMediator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseMediator"/> class.
    /// </summary>
    protected BaseMediator()
    {        
    }

    /// <inheritdoc/>
    public async virtual Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        cancellationToken.ThrowIfCancellationRequested();
        var handlers = GetEventHandlers<TEvent>();
        await Task.WhenAll(handlers.Select(handler => handler.HandleAsync(@event, cancellationToken)));
    }

    /// <inheritdoc/>
    public async virtual Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        cancellationToken.ThrowIfCancellationRequested();
        var handler = GetRequestHandler<TRequest>();
        var behaviors = GetRequestMiddleware().GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task MoveNextAsync(CancellationToken token)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            if (behaviors.MoveNext())
            {
                var current = behaviors.Current!;
                return current.InvokeAsync(request, Unsafe.As<RequestHandlerDelegate<VoidResult>>(MoveNextAsync), token);
            }

            return handler.HandleAsync(request, token);
        }

        await MoveNextAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async virtual Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>
    {
        cancellationToken.ThrowIfCancellationRequested();
        var handler = GetRequestHandler<TRequest, TResponse>();
        var behaviors = GetRequestMiddleware().GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task<TResponse> MoveNextAsync(CancellationToken token)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<TResponse>(cancellationToken);

            if (behaviors.MoveNext())
            {
                var current = behaviors.Current!;
                return current.InvokeAsync(request, MoveNextAsync, token);
            }

            return handler.HandleAsync(request, token);
        }

        return await MoveNextAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async virtual IAsyncEnumerable<TResponse> StreamAsync<TRequest, TResponse>(TRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>
    {
        var handler = GetStreamRequestHandler<TRequest, TResponse>();
        var behaviors = GetStreamRequestMiddleware().GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IAsyncEnumerable<TResponse> MoveNextAsync(CancellationToken token)
        {
            if (behaviors.MoveNext())
            {
                var current = behaviors.Current!;
                return current.InvokeAsync(request, MoveNextAsync, token);
            }

            return handler.HandleAsync(request, token);
        }

        var asyncEnumerable = MoveNextAsync(cancellationToken);
        await foreach (var response in asyncEnumerable.WithCancellation(cancellationToken))
        {
            yield return response;
        }
    }

    /// <summary>
    /// Gets the collection of event handlers for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to get handlers for.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IEventHandler{TEvent}"/> instances that can handle the specified event type.</returns>
    protected abstract IEnumerable<IEventHandler<TEvent>> GetEventHandlers<TEvent>() where TEvent : IEvent;

    /// <summary>
    /// Gets the request handler for the specified request type that does not return a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to get the handler for.</typeparam>
    /// <returns>An <see cref="IRequestHandler{TRequest}"/> instance that can handle the specified request type.</returns>
    protected abstract IRequestHandler<TRequest> GetRequestHandler<TRequest>() where TRequest : IRequest;

    /// <summary>
    /// Gets the request handler for the specified request type that returns a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to get the handler for.</typeparam>
    /// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
    /// <returns>An <see cref="IRequestHandler{TRequest, TResponse}"/> instance that can handle the specified request type and return the specified response type.</returns>
    protected abstract IRequestHandler<TRequest, TResponse> GetRequestHandler<TRequest, TResponse>() where TRequest : IRequest<TResponse>;

    /// <summary>
    /// Gets the stream request handler for the specified request type that returns an asynchronous stream of responses.
    /// </summary>
    /// <typeparam name="TRequest">The type of stream request to get the handler for.</typeparam>
    /// <typeparam name="TResponse">The type of response returned by the stream handler.</typeparam>
    /// <returns>An <see cref="IStreamRequestHandler{TRequest, TResponse}"/> instance that can handle the specified stream request type.</returns>
    protected abstract IStreamRequestHandler<TRequest, TResponse> GetStreamRequestHandler<TRequest, TResponse>() where TRequest : IStreamRequest<TResponse>;

    /// <summary>
    /// Gets the collection of request middleware that will be executed in the request pipeline.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IRequestMiddleware"/> instances that will process requests in the order returned.</returns>
    /// <remarks>
    /// The middleware is executed in the order returned by this method. Each middleware can perform operations before and after the request is processed by subsequent middleware or handlers.
    /// </remarks>
    protected abstract IEnumerable<IRequestMiddleware> GetRequestMiddleware();

    /// <summary>
    /// Gets the collection of stream request middleware that will be executed in the stream request pipeline.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IStreamRequestMiddleware"/> instances that will process stream requests in the order returned.</returns>
    /// <remarks>
    /// The middleware is executed in the order returned by this method. Each middleware can perform operations on the request and response stream.
    /// </remarks>
    protected abstract IEnumerable<IStreamRequestMiddleware> GetStreamRequestMiddleware();
}
