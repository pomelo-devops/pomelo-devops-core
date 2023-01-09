using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public class AgentPool
    {
        public long Id { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(64)]
        [ForeignKey(nameof(Organization))]
        public string ProjectId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual Project Organization { get; set; }

        public virtual ICollection<Agent> Agents { get; set; } = new List<Agent>();

        public virtual ICollection<JobStage> JobStages { get; set; }
    }
}
