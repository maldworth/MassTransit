﻿using System;

namespace MassTransit.Transports.OnRamp.Entities
{
    public class OnRampSweeper
    {
        public string OnRampName { get; set; }
        public string InstanceId { get; set; }
        public DateTime LastCheckinTime { get; set; }
        public TimeSpan CheckinInterval { get; set; }
    }
}
