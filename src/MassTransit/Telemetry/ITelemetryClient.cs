namespace MassTransit.Telemetry
{
    using Audit;
    using System;

    public interface ITelemetryClient
    {
        void TrackEvent(string eventName, MessageAuditMetadata metadata);
        void TrackException(string eventName, MessageAuditMetadata metadata, Exception exception);
        //void TrackSend(ISendTelemetry telemetry);
        //Guid OperationId { get; }
        //DateTime Started { get; }
        //TimeSpan Elapsed { get; }
    }
}
