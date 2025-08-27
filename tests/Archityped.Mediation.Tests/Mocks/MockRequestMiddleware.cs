namespace Archityped.Mediation.Tests.Mocks;

public class MockRequestMiddleware : IRequestMiddleware
{
    public Action<object>? BeforeHandler { get; set; }
    public Action<object, object?>? AfterHandler { get; set; }

    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
        where TRequest : IBaseRequest
    {
        BeforeHandler?.Invoke(request);
        var response = await next(cancellationToken);
        AfterHandler?.Invoke(request, response);
        return response;
    }
}