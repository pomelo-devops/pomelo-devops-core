using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.DevOps.Models;
using Pomelo.Workflow;
using Pomelo.Workflow.Models;
using Pomelo.Workflow.Models.ViewModels;
using Pomelo.Workflow.WorkflowHandler;


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
                    var jobInstance = await WorkflowManager
                        .GetWorkflowInstanceAsync(CurrentStep.WorkflowInstanceId, cancellationToken);
                    var job = await db.Jobs
                        .FirstOrDefaultAsync(x => x.PipelineWorkflowInstanceId == jobInstance.Id, cancellationToken);
                    var stageWorkflowId = CurrentStep.Arguments["StageWorkflowId"].ToObject<Guid>();
                    var latestVersion = await WorkflowManager.GetLatestVersionAsync(stageWorkflowId, cancellationToken: cancellationToken);
                    if (!latestVersion.HasValue)
                    {
                        throw new InvalidProgramException("Missing available workflow version.");
                    }

                    var instance = await WorkflowManager.CreateNewWorkflowInstanceAsync(stageWorkflowId, latestVersion.Value, CurrentStep.Arguments, cancellationToken);
                    db.JobWorkflowStages.Add(new JobWorkflowStage 
                    {
                        WorkflowInstanceId = instance.InstanceId,
                        JobId = job.Id,
                    });
                    await db.SaveChangesAsync(cancellationToken);
                    await WorkflowManager.StartWorkflowInstanceAsync(instance.InstanceId, cancellationToken);
                    break;
            }
        }

        public override Task<bool> IsAbleToMoveNextAsync(
            ConnectionType connectionToNextStep, 
            Shape currentNode,
            Shape nextNode, 
            CancellationToken cancellationToken = default)
        {
            if (connectionToNextStep.Type == "default")
            {
                return Task.FromResult(CurrentStep.Status == StepStatus.Succeeded);
            }
            else
            {
                return Task.FromResult(CurrentStep.Status == StepStatus.Failed);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            scope?.Dispose();
        }
    }
}
