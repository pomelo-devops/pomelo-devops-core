using System;
using System.ComponentModel.DataAnnotations.Schema;
using Pomelo.Workflow.Models.EntityFramework;

namespace Pomelo.DevOps.Models
{
    public class JobWorkflowStage
    {
        [ForeignKey(nameof(Job))]
        public Guid JobId { get; set; }

        public virtual Job Job { get; set; }

        [ForeignKey(nameof(WorkflowInstance))]
        public Guid WorkflowInstanceId { get; set; }

        public virtual DbWorkflowInstance WorkflowInstance { get; set; }
    }
}
