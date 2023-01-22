using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pomelo.Workflow.Models.EntityFramework;

namespace Pomelo.DevOps.Models
{
    public class PipelineDiagramStage
    {
        public Guid Id { get; set; }

        [MaxLength(64)]
        [ForeignKey(nameof(Pipeline))]
        public string PipelineId { get; set; }

        public virtual Pipeline Pipeline { get; set; }

        [ForeignKey(nameof(Workflow))]
        public Guid WorkflowId { get; set; }

        public DbWorkflow Workflow { get; set; }
    }
}
