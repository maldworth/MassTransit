namespace MassTransit.SignalR
{
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using MassTransit.Scoping;
    using System;
    using System.Collections.Generic;

    public static class MassTransitSignalRConfigurationExtensions
    {
        public static void CreateBackplaneEndpointsByHub<T>(this IBusFactoryConfigurator configurator, IServiceProvider serviceProvider, IHost host, IReadOnlyDictionary<Type, IReadOnlyList<Type>> hubConsumers, Action<T> configureEndpoint = null)
            where T : class, IReceiveEndpointConfigurator
        {
            var factoryType = typeof(ScopeConsumerFactory<>);
            var consumerConfiguratorType = typeof(ConsumerConfigurator<>);

            foreach (var hub in hubConsumers)
            {
                var queueName = host.Topology.CreateTemporaryQueueName($"signalRBackplane-{hub.Key.Name}-");

                configurator.ReceiveEndpoint(queueName, e =>
                {
                    configureEndpoint?.Invoke((T)e);

                    // Loop through our 5 hub consumers and register them to the temporary endpoint
                    foreach (var consumer in hub.Value)
                    {
                        IConsumerScopeProvider scopeProvider = new DependencyInjectionConsumerScopeProvider(serviceProvider);

                        var concreteFactoryType = factoryType.MakeGenericType(consumer);

                        var consumerFactory = Activator.CreateInstance(concreteFactoryType, scopeProvider);

                        var concreteConsumerConfiguratorType = consumerConfiguratorType.MakeGenericType(consumer);

                        var consumerConfigurator = Activator.CreateInstance(concreteConsumerConfiguratorType, consumerFactory, e);

                        e.AddEndpointSpecification((IReceiveEndpointSpecification)consumerConfigurator);
                    }
                });
            }
            
        }

        public static void CreateBackplaneEndpointForAllHubs<T>(this IBusFactoryConfigurator configurator, IServiceProvider serviceProvider, IHost host, IReadOnlyDictionary<Type, IReadOnlyList<Type>> hubConsumers, Action<T> configureEndpoint = null)
            where T : class, IReceiveEndpointConfigurator
        {
            var factoryType = typeof(ScopeConsumerFactory<>);
            var consumerConfiguratorType = typeof(ConsumerConfigurator<>);

            var queueName = host.Topology.CreateTemporaryQueueName($"signalRBackplaneAllHubs-");

            configurator.ReceiveEndpoint(queueName, e =>
            {
                configureEndpoint?.Invoke((T)e);

                foreach (var hub in hubConsumers)
                {
                    // Loop through our 5 hub consumers and register them to the temporary endpoint
                    foreach (var consumer in hub.Value)
                    {
                        IConsumerScopeProvider scopeProvider = new DependencyInjectionConsumerScopeProvider(serviceProvider);

                        var concreteFactoryType = factoryType.MakeGenericType(consumer);

                        var consumerFactory = Activator.CreateInstance(concreteFactoryType, scopeProvider);

                        var concreteConsumerConfiguratorType = consumerConfiguratorType.MakeGenericType(consumer);

                        var consumerConfigurator = Activator.CreateInstance(concreteConsumerConfiguratorType, consumerFactory, e);

                        e.AddEndpointSpecification((IReceiveEndpointSpecification)consumerConfigurator);
                    }
                }

            });
        }
    }
}
