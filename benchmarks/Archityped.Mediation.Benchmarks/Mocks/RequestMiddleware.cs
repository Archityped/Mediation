namespace Archityped.Mediation.Benchmarks.Mocks;

public class RequestMiddleware : IRequestMiddleware
{
    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
        where TRequest : IBaseRequest
    {
        return await next(cancellationToken);
    }
}