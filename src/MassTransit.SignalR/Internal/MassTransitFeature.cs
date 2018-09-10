namespace MassTransit.SignalR.Internal
{
    using MassTransit.SignalR.Utils;
    using System;

    public class MassTransitFeature : IMassTransitFeature
    {
        public ConcurrentHashSet<string> Groups { get; } = new ConcurrentHashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
