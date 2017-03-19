namespace MassTransit.Telemetry.Observers
{
    using Audit;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Util;
    using Util.Scanning;

    public class TelemetryPublishObserver : IPublishObserver
    {
        readonly ITelemetryClient _telemetryClient;
        readonly CompositeFilter<SendContext> _filter;
        readonly ISendMetadataFactory _metadataFactory;

        public TelemetryPublishObserver(ITelemetryClient telemetryClient, ISendMetadataFactory metadataFactory, CompositeFilter<SendContext> filter)
        {
            _metadataFactory = metadataFactory;
            _telemetryClient = telemetryClient;
            _filter = filter;
        }

        public Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            if (!_filter.Matches(context))
                return TaskUtil.Completed;

            var metadata = _metadataFactory.CreateAuditMetadata(context);
            string eventName = $"{typeof(T).Namespace}.{typeof(T).Name}"; // This should be the name of the command/event (maybe extract it from topology??)
            _telemetryClient.TrackEvent(eventName, metadata);

            return TaskUtil.Completed;
        }

        public Task PrePublish<T>(PublishContext<T> context) where T : class => TaskUtil.Completed;

        public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            if (!_filter.Matches(context))
                return TaskUtil.Completed;

            var metadata = _metadataFactory.CreateAuditMetadata(context);
            string eventName = $"{typeof(T).Namespace}.{typeof(T).Name}"; // This should be the name of the command/event (maybe extract it from topology??)
            _telemetryClient.TrackException(eventName, metadata, exception);

            return TaskUtil.Completed;
        }
    }
}
