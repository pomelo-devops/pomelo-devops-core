using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Pomelo.DevOps.Models.ViewModels;
using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Models
{
    public class JobStep
    {
        [YamlMember(Alias = "guid", Order = 5)]
        public Guid Id { get; set; }

        [YamlMember(Alias = "order", Order = 5)]
        public int Order { get; set; }

        [YamlMember(Alias = "step", Order = 1)]
        [MaxLength(64)]
        public string StepId { get; set; }

        [YamlMember(Alias = "version", Order = 2)]
        [MaxLength(64)]
        public string Version { get; set; }

        [YamlMember(Alias = "method", Order = 3)]
        [MaxLength(64)]
        public string Method { get; set; }

        [YamlMember(Alias = "name", Order = 4)]
        [MaxLength(64)]
        public string Name { get; set; }

        [YamlIgnore]
        [ForeignKey(nameof(PipelineJobStage))]
        public Guid PipelineJobStageId { get; set; }

        [YamlIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public virtual JobStage PipelineJobStage { get; set; }

        [YamlMember(Alias = "status", Order = 6)]
        public PipelineJobStatus Status { get; set; }

        [YamlMember(Alias = "condition", Order = 7, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public RunCondition Condition { get; set; }

        [MaxLength(64)]
        [YamlMember(Alias = "condition_variable", Order = 7)]
        public string ConditionVariable { get; set; }

        [YamlMember(Alias = "started_at", Order = 8)]
        public DateTime? StartedAt { get; set; }

        [YamlMember(Alias = "finished_at", Order = 9)]
        public DateTime? FinishedAt { get; set; }

        [YamlMember(Alias = "timeout", Order = 10)]
        public int Timeout { get; set; } = -1;

        [YamlMember(Alias = "retry", Order = 11)]
        public int Retry { get; set; } = 0;

        [YamlMember(Alias = "attempts", Order = 12)]
        public int Attempts { get; set; } = 0;

        [YamlIgnore]
        public string ArgumentsString { get; set; } = "{}";

        [NotMapped]
        [YamlMember(Alias = "arguments", Order = 14)]
        public Dictionary<string, string> Arguments 
        {
            get => JsonConvert.DeserializeObject<Dictionary<string, string>>(ArgumentsString);
            set
            {
                ArgumentsString = JsonConvert.SerializeObject(value);
            }
        }

        [YamlMember(Alias = "error_handling_mode", Order = 13, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public ErrorHandlingMode ErrorHandlingMode { get; set; }
    }
}
