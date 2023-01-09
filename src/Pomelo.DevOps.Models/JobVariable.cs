using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public class JobVariable
    {
        [ForeignKey(nameof(PipelineJob))]
        public Guid PipelineJobId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual Job PipelineJob { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
