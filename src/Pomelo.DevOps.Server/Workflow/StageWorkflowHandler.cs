using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.DevOps.Models;
using Pomelo.Workflow;
using Pomelo.Workflow.Models;
using Pomelo.Workflow.Models.ViewModels;
using Pomelo.Workflow.WorkflowHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pomelo.DevOps.Server.Workflow
{
    [WorkflowHandler("stage")]
    public class StageWorkflowHandler : WorkflowHandlerBase
    {
        private readonly IServiceScope scope;

        public StageWorkflowHandler(
            IServiceProvider services,
            WorkflowManager workflowManager,
            WorkflowInstanceStep step) 
            : base(services, workflowManager, step)
        {
            scope = services.CreateScope();
        }

        public override async Task OnPreviousStepFinishedAsync(
            IEnumerable<ConnectionTypeWithDeparture> stepStatuses, 
            CancellationToken cancellationToken)
        {
            if (stepStatuses.All(x => x.DepartureStep.Status >= StepStatus.Failed))
            {
                await WorkflowManager.UpdateWorkflowStepAsync(CurrentStep.Id, StepStatus.InProgress, null, null, cancellationToken);
            }
        }

        public override async Task OnStepStatusChangedAsync(
            StepStatus newStatus, 
            StepStatus previousStatus,
            CancellationToken cancellationToken)
        {
            using var db = scope.ServiceProvider.GetRequiredService<PipelineContext>();

            switch (CurrentStep.Status)
            {
                case StepStatus.InProgress:
                    var instance = await WorkflowManager
                        .GetWorkflowInstanceAsync(CurrentStep.WorkflowInstanceId);
                    var workflowId = CurrentStep.Arguments["StageWorkflowId"].ToObject<Guid>();
                    db.JobWorkflowStages.Add(new JobWorkflowStage 
                    {
                        
                    });
                    break;
            }

            // TOOD: Implement
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            base.Dispose();

            scope?.Dispose();
        }
    }
}
