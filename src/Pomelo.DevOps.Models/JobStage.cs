using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pomelo.DevOps.Models.ViewModels;
using Pomelo.Workflow.Models;
using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Models
{
    public class JobStage
    {
        [YamlMember(Alias = "id", Order = 1)]
        public Guid Id { get; set; }

        [YamlMember(Alias = "order", Order = 2, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public int Order { get; set; }

        [ForeignKey(nameof(PipelineJob))]
        [YamlMember(Alias = "job", Order = 2, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public Guid PipelineJobId { get; set; }

        // The not mapped properties will be copied from PipelineJob for agent use.
        [NotMapped]
        [YamlMember(Alias = "job_number", Order = 2, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public long JobNumber { get; set; }

        [NotMapped]
        [YamlMember(Alias = "project", Order = 2, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public string Project { get; set; }

        [NotMapped]
        [YamlMember(Alias = "pipeline", Order = 2, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public string Pipeline { get; set; }

        [NotMapped]
        [YamlMember(Alias = "type", Order = 2, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public PipelineType Type { get; set; }

        [YamlIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public virtual Job PipelineJob { get; set; }

        [MaxLength(64)]
        [YamlMember(Alias = "name", Order = 3)]
        public string Name { get; set; }

        [ForeignKey(nameof(AgentPool))]
        [YamlMember(Alias = "pool", Order = 4)]
        public long? AgentPoolId { get; set; }

        [YamlIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public virtual AgentPool AgentPool { get; set; }

        [YamlMember(Alias = "isolation_level", Order = 5)]
        public IsolationLevel IsolationLevel { get; set; }

        [ForeignKey(nameof(Agent))]
        public Guid? AgentId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [YamlIgnore]
        public virtual Agent Agent { get; set; }

        [YamlIgnore]
        public DateTime? StartedAt { get; set; }

        [YamlIgnore]
        public DateTime? FinishedAt { get; set; }

        [YamlMember(Alias = "status", Order = 6)]
        public PipelineJobStatus Status { get; set; }

        [YamlIgnore]
        public RunCondition Condition { get; set; }

        [YamlIgnore]
        [MaxLength(64)]
        public string ConditionVariable { get; set; }

        [YamlMember(Alias = "timeout", Order = 7)]
        public int Timeout { get; set; } = -1;

        [YamlMember(Alias = "identifier", Order = 8)]
        public int? Identifier { get; set; } = null;

        [YamlMember(Alias = "steps", Order = 9, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public virtual ICollection<JobStep> Steps { get; set; }

        [YamlIgnore]
        [ForeignKey(nameof(PipelineDiagramStage))]
        public Guid? PipelineDiagramStageId { get; set; }

        [YamlIgnore]
        public virtual PipelineDiagramStage PipelineDiagramStage { get; set; }

        [NotMapped]
        [YamlMember(Alias = "diagram", Order = 9, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public virtual Diagram Diagram { get; set; }
    }
}
