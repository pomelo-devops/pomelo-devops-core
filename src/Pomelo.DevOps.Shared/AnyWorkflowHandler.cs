using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.Workflow;
using Pomelo.Workflow.Models;
using Pomelo.Workflow.Models.ViewModels;
using Pomelo.Workflow.WorkflowHandler;

namespace Pomelo.DevOps.Shared
{
    [WorkflowHandler("any")]
    public class AnyWorkflowHandler : WorkflowHandlerBase
    {
        public AnyWorkflowHandler(
            IServiceProvider services,
            WorkflowManager workflowManager,
            WorkflowInstanceStep step)
            : base(services, workflowManager, step)
        { }

        public override async Task OnPreviousStepFinishedAsync(
            IEnumerable<ConnectionTypeWithDeparture> stepStatuses, 
            CancellationToken cancellationToken)
        {
            if (CurrentStep.Status < StepStatus.Failed)
            {
                await WorkflowManager.UpdateWorkflowStepAsync(
                    CurrentStep.Id, 
                    StepStatus.Succeeded,
                    null,
                    null, 
                    cancellationToken);
            }
        }

        public override Task OnStepStatusChangedAsync(StepStatus newStatus, StepStatus previousStatus, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
