namespace MassTransit.SignalR.Internal
{
    using MassTransit.SignalR.Utils;

    public interface IMassTransitFeature
    {
        ConcurrentHashSet<string> Groups { get; }
    }
}
