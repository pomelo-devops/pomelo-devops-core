using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pomelo.Workflow.Models.EntityFramework;

namespace Pomelo.DevOps.Models
{
    public enum PipelineJobTriggerType
    {
        Manual,
        API,
        Trigger
    }

    public enum PipelineJobStatus
    {
        Pending,
        Waiting,
        Running,
        Failed,
        Skipped,
        Succeeded
    }

    public class Job
    {
        public Guid Id { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        public string Description { get; set; }

        public PipelineJobTriggerType TriggerType { get; set; }

        [MaxLength(64)]
        [ForeignKey(nameof(Pipeline))]
        public string PipelineId { get; set; }

        public long Number { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual Pipeline Pipeline { get; set; }

        public PipelineType Type { get; set; }

        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartedAt { get; set; } = null;

        public DateTime? FinishedAt { get; set; } = null;

        public PipelineJobStatus Status { get; set; }

        [MaxLength(64)]
        public string TriggerName { get; set; } 

        public int Timeout { get; set; } = -1;

        public int CurrentStageOrder { get; set; } = 0;

        public virtual ICollection<JobStage> LinearStages { get; set; }

        [ForeignKey(nameof(DiagramWorkflowInstance))]
        public Guid? DiagramWorkflowInstanceId { get; set; }

        public virtual DbWorkflowInstance DiagramWorkflowInstance { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual ICollection<JobVariable> Variables { get; set; }

        public virtual ICollection<JobLabel> Labels { get; set; }

        public virtual ICollection<JobWorkflowStage> WorkflowStages { get; set; }
    }
}
