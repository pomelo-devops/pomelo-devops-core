using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Models
{
    public class PipelineTrigger
    {
        public Guid Id { get; set; }

        [ForeignKey(nameof(Pipeline))]
        [MaxLength(64)]
        public string PipelineId { get; set; }

        [JsonIgnore]
        public virtual Pipeline Pipeline { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(64)]
        [ForeignKey(nameof(Provider))]
        public string ProviderId { get; set; }

        [JsonIgnore]
        public virtual TriggerProvider Provider { get; set; }

        [MaxLength(128)]
        public string ExternalId { get; set; }

        public bool Enabled { get; set; }
    }
}
