using System;
using Pomelo.Workflow;
using Pomelo.Workflow.Storage;

namespace Pomelo.DevOps.Agent.Workflow
{
    public class AgentWorkflowManager : WorkflowManager
    {
        public AgentWorkflowManager(
            IServiceProvider services, 
            IWorkflowStorageProvider storage) 
            : base(services, storage)
        { }
    }
}
