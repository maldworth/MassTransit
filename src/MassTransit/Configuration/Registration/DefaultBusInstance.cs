namespace MassTransit.Registration
{
    using System;
    using Configuration;


    public class DefaultBusInstance :
        IBusInstance
    {
        public DefaultBusInstance(IBusControl busControl)
        {
            BusControl = busControl;
        }

        public Type InstanceType => typeof(IBus);
        public IBus Bus => BusControl;
        public IBusControl BusControl { get; }

        public IHostConfiguration HostConfiguration => default;
    }
}
