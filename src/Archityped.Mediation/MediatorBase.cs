namespace Archityped.Mediation;

/// <summary>
/// Represents a base implementation for mediators.
/// </summary>
public abstract class MediatorBase : IMediator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorBase"/> class.
    /// </summary>
    /// <returns>A new instance of the <see cref="MediatorBase"/> class.</returns>
    protected MediatorBase()
    {
    }

    /// <inheritdoc/>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public async virtual Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        cancellationToken.ThrowIfCancellationRequested();
        var publisher = GetNotificationPublisher();
        await publisher.PublishAsync(notification, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public async virtual Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        cancellationToken.ThrowIfCancellationRequested();
        var handler = GetRequestHandler<TRequest>();
        var behaviors = GetRequestMiddleware();
        var index = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task<VoidResult> MoveNextAsync(CancellationToken token)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<VoidResult>(cancellationToken);

            if (index < behaviors.Count)
            {
                var current = behaviors[index++];
                return current.InvokeAsync(request, MoveNextAsync, token);
            }

            return MoveFinalAsync(handler, request, cancellationToken);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static async Task<VoidResult> MoveFinalAsync(IRequestHandler<TRequest> handler, TRequest req, CancellationToken token)
            {
                await handler.HandleAsync(req, token).ConfigureAwait(false);
                return VoidResult.Instance;
            }
        }

        await MoveNextAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public async virtual Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>
    {
        cancellationToken.ThrowIfCancellationRequested();
        var handler = GetRequestHandler<TRequest, TResponse>();
        var behaviors = GetRequestMiddleware();
        var index = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task<TResponse> MoveNextAsync(CancellationToken token)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<TResponse>(cancellationToken);

            if (index < behaviors.Count)
            {
                var current = behaviors[index++];
                return current.InvokeAsync(request, MoveNextAsync, token);
            }

            return handler.HandleAsync(request, token);
        }

        return await MoveNextAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public virtual async IAsyncEnumerable<TResponse> StreamAsync<TRequest, TResponse>(TRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>
    {
        var handler = GetStreamRequestHandler<TRequest, TResponse>();
        var behaviors = GetStreamRequestMiddleware();
        var index = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IAsyncEnumerable<TResponse> MoveNextAsync(CancellationToken token)
        {
            if (index < behaviors.Count)
            {
                var current = behaviors[index++];
                return current.InvokeAsync(request, MoveNextAsync, token);
            }

            return handler.HandleAsync(request, token);
        }

        var asyncEnumerable = MoveNextAsync(cancellationToken)
            .ConfigureAwait(false)
            .WithCancellation(cancellationToken);

        await foreach (var response in asyncEnumerable)
        {
            yield return response;
        }
    }

    /// <summary>
    /// Gets the notification publishers that will distribute notifications.
    /// </summary>
    /// <returns>An <see cref="INotificationPublisher"/> instances that will process notifications.</returns>
    protected abstract INotificationPublisher GetNotificationPublisher();

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
    /// <returns>An <see cref="IReadOnlyList{T}"/> of <see cref="IRequestMiddleware"/> instances that will process requests in the order returned.</returns>
    /// <remarks>
    /// The middleware is executed in the order returned by this method. Each middleware can perform operations before and after the request is processed by subsequent middleware or handlers.
    /// </remarks>
    protected abstract IReadOnlyList<IRequestMiddleware> GetRequestMiddleware();

    /// <summary>
    /// Gets the collection of stream request middleware that will be executed in the stream request pipeline.
    /// </summary>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of <see cref="IStreamRequestMiddleware"/> instances that will process stream requests in the order returned.</returns>
    /// <remarks>
    /// The middleware is executed in the order returned by this method. Each middleware can perform operations on the request and response stream.
    /// </remarks>
    protected abstract IReadOnlyList<IStreamRequestMiddleware> GetStreamRequestMiddleware();
}
