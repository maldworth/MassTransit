﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace MassTransit.Transports.OnRamp
{
    public interface IClusterManager
    {
        Task CheckIn(Guid requestorId, CancellationToken cancellationToken);
        Task RecoverMessagesAndCleanup(Guid requestorId, CancellationToken cancellationToken);
    }
}
