using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pomelo.Workflow.Models;
using Pomelo.Workflow.Models.ViewModels;
using Pomelo.Workflow.Storage;

namespace Pomelo.DevOps.Agent.Workflow
{
    public class AgentWorkflowStorageProvider : IWorkflowStorageProvider
    {
        public Task<Guid> CreateWorkflowAsync(CreateWorkflowRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> CreateWorkflowInstanceAsync(Guid id, int version, Dictionary<string, JToken> arguments, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task CreateWorkflowInstanceConnectionAsync(WorkflowInstanceConnection request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> CreateWorkflowStepAsync(Guid instanceId, WorkflowInstanceStep step, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateWorkflowVersion(Guid workflowId, CreateWorkflowVersionRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WorkflowInstanceStep>> GetInstanceStepsAsync(Guid instanceId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int?> GetLatestVersionAsync(Guid workflowId, WorkflowVersionStatus? status = WorkflowVersionStatus.Available, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<GetPreviousStepsResult> GetPreviousStepsAsync(Guid stepId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<WorkflowInstanceStep> GetStepByShapeId(Guid instanceId, Guid shapeId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Pomelo.Workflow.Models.Workflow> GetWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<WorkflowInstance> GetWorkflowInstanceAsync(Guid instanceId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WorkflowInstanceConnection>> GetWorkflowInstanceConnectionsAsync(Guid instanceId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<GetWorkflowInstanceResult>> GetWorkflowInstancesAsync(Guid workflowId, int? version, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<WorkflowInstanceStep> GetWorkflowInstanceStepAsync(Guid stepId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Pomelo.Workflow.Models.Workflow>> GetWorkflowsAsync(string name = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<WorkflowVersion> GetWorkflowVersionAsync(Guid workflowId, int version, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<GetWorkflowVersionResult>> GetWorkflowVersionsAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateWorkflowAsync(Guid workflowId, UpdateWorkflowRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateWorkflowInstanceResult> UpdateWorkflowInstanceAsync(Guid instanceId, WorkflowStatus status, Action<Dictionary<string, JToken>> updateArgumentsDelegate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateWorkflowInstanceUpdateTimeAsync(Guid instanceId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateWorkflowStepResult> UpdateWorkflowStepAsync(Guid stepId, StepStatus status, Action<Dictionary<string, JToken>> updateArgumentsDelegate, string error = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateWorkflowVersionStatusAsync(Guid workflowId, int version, WorkflowVersionStatus status, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
