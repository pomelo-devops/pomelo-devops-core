using System;

namespace Pomelo.DevOps.Agent.Workflow
{
    public class WorkflowContext
    {
        public string ProjectId { get; set; }

        public string PipelineId { get; set; }

        public long JobNumber { get; set; }

        public Guid WorkflowInstanceId { get; set; }
    }
}
