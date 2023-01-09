using System;

namespace Pomelo.DevOps.Models
{
    public enum MetricsType
    {
        Memory,
        Processor,
        Network,
        Disk
    }

    public class Metrics
    {
        public Guid Id { get; set; }

        public long Timestamp { get; set; }

        public Guid AgentId { get; set; }

        public double Value { get; set; }

        public MetricsType Type { get; set; }
    }
}
