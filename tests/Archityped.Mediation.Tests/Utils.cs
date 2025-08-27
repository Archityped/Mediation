namespace Archityped.Mediation.Tests;

internal static class Utils
{
    /// <summary>
    /// Creates an IMediator instance with the specified configuration.
    /// </summary>
    public static IMediator CreateMediator(Action<MediatorConfiguration> configuration)
    {
        var serviceProvider = new ServiceCollection()
            .AddMediator(configuration!)
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<IMediator>();
    }
}
