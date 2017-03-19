namespace MassTransit.ApplicationInsightsIntegration
{
    using Audit;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using System;
    using Telemetry;

    public class ApplicationInsightsTelemetryClient : ITelemetryClient
    {
        private readonly TelemetryClient _client;

        public ApplicationInsightsTelemetryClient(TelemetryConfiguration configuration = null)
        {
            _client = configuration != null ? new TelemetryClient(configuration) : new TelemetryClient();
            //https://docs.microsoft.com/en-us/azure/application-insights/app-insights-api-custom-events-metrics
            // Perhaps set the operationId for each context, probably should be done in the

            //https://docs.microsoft.com/en-us/azure/application-insights/app-insights-export-data-model

            //http://apmtips.com/blog/2015/01/05/track-dependencies-in-console-application/

            // For Desktop Apps
            //https://docs.microsoft.com/en-us/azure/application-insights/app-insights-windows-desktop
        }


        public void TrackEvent(string eventName, MessageAuditMetadata metadata)
        {
            var telemetry = new EventTelemetry
            {
                Name = eventName,
                Timestamp = DateTimeOffset.Now
            };

            GatherMetadata(telemetry.Context, metadata);

            _client.TrackEvent(telemetry);
        }

        public void TrackException(string eventName, MessageAuditMetadata metadata, Exception exception)
        {
            var telemetry = new ExceptionTelemetry
            {
                Exception = exception,
                Timestamp = DateTimeOffset.Now
            };

            GatherMetadata(telemetry.Context, metadata);

            _client.TrackException(telemetry);
        }

        private void GatherMetadata(TelemetryContext context, MessageAuditMetadata metadata)
        {
            context.Operation.Id = metadata.CorrelationId.ToString();
            context.Operation.ParentId = metadata.ConversationId.ToString();

            context.Properties.Add(nameof(metadata.ContextType), metadata.ContextType);

            context.Properties.Add(nameof(metadata.DestinationAddress), metadata.DestinationAddress);
            context.Properties.Add(nameof(metadata.InitiatorId), metadata.InitiatorId?.ToString());
            context.Properties.Add(nameof(metadata.MessageId), metadata.MessageId?.ToString());
            context.Properties.Add(nameof(metadata.RequestId), metadata.RequestId?.ToString());
            context.Properties.Add(nameof(metadata.ResponseAddress), metadata.ResponseAddress);
            context.Properties.Add(nameof(metadata.SourceAddress), metadata.SourceAddress);
            context.Properties.Add(nameof(metadata.FaultAddress), metadata.FaultAddress);

            foreach (var kvp in metadata.Headers)
            {
                context.Properties.Add(kvp);
            }

            foreach (var kvp in metadata.Custom)
            {
                context.Properties.Add(kvp);
            }
        }
    }
}
