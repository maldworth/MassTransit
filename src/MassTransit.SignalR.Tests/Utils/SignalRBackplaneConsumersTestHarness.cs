namespace MassTransit.SignalR.Tests
{
    using MassTransit.Testing;
    using MassTransit.Testing.MessageObservers;
    using Microsoft.AspNetCore.SignalR;
    using System.Collections.Generic;

    public class SignalRBackplaneConsumersTestHarness<THub>
        where THub : Hub
    {
        readonly ConsumersTestHarness _consumersTestHarness;
        readonly IList<IHubManagerConsumerFactory<THub>> _hubManagerConsumerFactories;

        public ReceivedMessageList Consumed => _consumersTestHarness.Consumed;

        public HubLifetimeManager<THub> HubLifetimeManager { get; private set; }

        public SignalRBackplaneConsumersTestHarness(BusTestHarness testHarness, string queueName)
        {
            _consumersTestHarness = new ConsumersTestHarness(testHarness, queueName);
            _hubManagerConsumerFactories = new List<IHubManagerConsumerFactory<THub>>();
        }

        public void Add<TConsumer>(HubLifetimeManagerConsumerFactory<TConsumer, THub> hubConsumerFactory)
            where TConsumer : class, IConsumer
        {
            _consumersTestHarness.Add(hubConsumerFactory);

            _hubManagerConsumerFactories.Add(hubConsumerFactory);
        }

        public void SetHubLifetimeManager(HubLifetimeManager<THub> hubLifetimeManager)
        {
            foreach (var factory in _hubManagerConsumerFactories)
            {
                factory.HubLifetimeManager = hubLifetimeManager;
            }

            HubLifetimeManager = hubLifetimeManager;
        }
    }
}
