﻿// Copyright 2007-2017 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.RabbitMqTransport.Builders
{
    using System;
    using Topology;


    public class StaticRabbitMqPublishTopology<TMessage> 
        where TMessage : class
    {
        readonly Uri _address;

        public StaticRabbitMqPublishTopology(Uri address)
        {
            _address = address;
        }

        public bool TryGetPublishAddress(IRabbitMqHostTopology topology, TMessage message, out Uri address)
        {
            address = _address;
            return true;
        }

        public void ApplyPublishTopology(RabbitMqSendContext<TMessage> context)
        {
            throw new NotImplementedException();
        }
    }
}