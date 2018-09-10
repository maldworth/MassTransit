// Copyright 2007-2017 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Testing
{
    using Decorators;
    using MessageObservers;
    using System;
    using System.Collections.Generic;

    public class ConsumersTestHarness
    {
        readonly BusTestHarness _testHarness;
        readonly IList<Action<IReceiveEndpointConfigurator>> _consumerConfigurations;
        public ReceivedMessageList Consumed { get; }

        public ConsumersTestHarness(BusTestHarness testHarness, string queueName)
        {
            Consumed = new ReceivedMessageList(testHarness.TestTimeout);
            _testHarness = testHarness;
            _consumerConfigurations = new List<Action<IReceiveEndpointConfigurator>>();

            if (string.IsNullOrWhiteSpace(queueName))
                testHarness.OnConfigureReceiveEndpoint += ConfigureReceiveEndpoint;
            else
                testHarness.OnConfigureBus += configurator => ConfigureNamedReceiveEndpoint(configurator, queueName);
        }

        public virtual void Add<TConsumer>(IConsumerFactory<TConsumer> consumerFactory)
            where TConsumer : class, IConsumer
        {
            var decorator = new TestConsumerFactoryDecorator<TConsumer>(consumerFactory, Consumed);

            _consumerConfigurations.Add(cfg => cfg.Consumer(decorator));
        }

        protected virtual void ConfigureReceiveEndpoint(IReceiveEndpointConfigurator configurator)
        {
            foreach (var cfg in _consumerConfigurations)
            {
                cfg(configurator);
            }
        }

        protected virtual void ConfigureNamedReceiveEndpoint(IBusFactoryConfigurator configurator, string queueName)
        {
            configurator.ReceiveEndpoint(queueName, x =>
            {
                foreach (var cfg in _consumerConfigurations)
                {
                    cfg(x);
                }
            });
        }
    }
}