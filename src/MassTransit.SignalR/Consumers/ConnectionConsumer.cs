namespace MassTransit.SignalR.Consumers
{
    using MassTransit.SignalR.Contracts;
    using MassTransit.SignalR.Utils;
    using Microsoft.AspNetCore.SignalR;
    using System;
    using System.Threading.Tasks;

    public class ConnectionConsumer<THub> : IConsumer<Connection<THub>> where THub : Hub
    {
        private readonly MassTransitHubLifetimeManager<THub> _hubLifetimeManager;

        public ConnectionConsumer(HubLifetimeManager<THub> hubLifetimeManager)
        {
            _hubLifetimeManager = hubLifetimeManager as MassTransitHubLifetimeManager<THub> ?? throw new ArgumentNullException(nameof(hubLifetimeManager), "HubLifetimeManager<> must be of type MassTransitHubLifetimeManager<>");
        }

        public Task Consume(ConsumeContext<Connection<THub>> context)
        {
            var message = new Lazy<SerializedHubMessage>(() => context.Message.Messages.ToSerializedHubMessage());

            var connection = _hubLifetimeManager.Connections[context.Message.ConnectionId];
            if (connection != null)
            {
                return connection.WriteAsync(message.Value).AsTask();
            }

            // ConnectionId doesn't exist on this server, skipping
            return Task.CompletedTask;
        }
    }
}
