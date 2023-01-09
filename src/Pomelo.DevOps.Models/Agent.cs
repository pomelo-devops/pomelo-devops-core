using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public enum AgentStatus
    {
        Idle,
        Busy,
        Offline
    }

    public class Agent
    {
        public Guid Id { get; set; }

        [ForeignKey(nameof(AgentPool))]
        public long AgentPoolId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual AgentPool AgentPool { get; set; }

        [MaxLength(64)]
        public string Description { get; set; }

        public DateTime HeartBeat { get; set; } = new DateTime(0);

        [MaxLength(64)]
        public string Address { get; set; }

        [MaxLength(64)]
        public string ClientVersion { get; set; }

        public AgentStatus Status { get; set; }

        [ForeignKey(nameof(CurrentJobStage))]
        public Guid? CurrentJobStageId { get; set; }

        [JsonIgnore]
        public virtual JobStage CurrentJobStage { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual ICollection<JobStage> JobStages { get; set; }
    }
}
