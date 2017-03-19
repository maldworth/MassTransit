namespace MassTransit.Telemetry.Observers
{
    using Audit;
    using System;
    using System.Threading.Tasks;
    using Util.Scanning;

    public class TelemetryConsumeObserver : IConsumeObserver
    {
        readonly ITelemetryClient _telemetryClient;
        readonly CompositeFilter<SendContext> _filter;
        readonly ISendMetadataFactory _metadataFactory;

        public TelemetryConsumeObserver(ITelemetryClient telemetryClient, ISendMetadataFactory metadataFactory, CompositeFilter<SendContext> filter)
        {
            _metadataFactory = metadataFactory;
            _telemetryClient = telemetryClient;
            _filter = filter;
        }

        public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            throw new NotImplementedException();
        }

        public Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
            throw new NotImplementedException();
        }

        public Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
