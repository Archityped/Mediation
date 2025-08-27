namespace Archityped.Mediation;

/// <summary>
/// Represents a request that returns a stream of responses of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IStreamRequest<out TResponse> : IBaseStreamRequest
{
}
