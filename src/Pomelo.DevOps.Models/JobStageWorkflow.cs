using System;
using System.ComponentModel.DataAnnotations.Schema;
using Pomelo.Workflow.Models.EntityFramework;

namespace Pomelo.DevOps.Models
{
    public class JobStageWorkflow
    {
        [ForeignKey(nameof(JobStage))]
        public Guid JobStageId { get; set; }

        public virtual JobStage JobStage { get; set; }

        [ForeignKey(nameof(WorkflowInstance))]
        public Guid WorkflowInstanceId { get; set; }

        public virtual DbWorkflowInstance WorkflowInstance { get; set; }
    }
}
