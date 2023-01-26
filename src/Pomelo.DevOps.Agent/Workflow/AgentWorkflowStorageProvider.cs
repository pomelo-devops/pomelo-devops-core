using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pomelo.Workflow.Models;
using Pomelo.Workflow.Models.ViewModels;
using Pomelo.Workflow.Storage;
using Newtonsoft.Json;
using System.Text;
using Pomelo.DevOps.Models.ViewModels;

namespace Pomelo.DevOps.Agent.Workflow
{
    public class AgentWorkflowStorageProvider : IWorkflowStorageProvider, IDisposable
    {
        private readonly ConfigManager _config;
        private readonly HttpClient _client;
        private readonly WorkflowContext _context;
        private static readonly string AssemblyDirectory
            = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string BuildVersionPath
            = Path.Combine(AssemblyDirectory, "build.txt");
        private static readonly string CilentVersion = File.Exists(BuildVersionPath)
            ? File.ReadAllText(BuildVersionPath).Trim()
            : null;

        public AgentWorkflowStorageProvider(
            WorkflowContext context,
            ConfigManager config) 
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13
                | SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls;

            _config = config;
            _context = context;
            _client = new HttpClient() { BaseAddress = new Uri(_config.Config.Server.TrimEnd('/')) };
            _client.DefaultRequestHeaders.Add("Authorization", "Token " + _config.Config.PersonalAccessToken);
            if (CilentVersion != null)
            {
                _client.DefaultRequestHeaders.Add("X-Pomelo-Agent-Version", CilentVersion);
            }
        }

        #region Base

        public async Task<TResponse> RequestAsync<TRequest, TResponse>(
            HttpMethod method,
            string endpoint,
            TRequest request = null,
            CancellationToken cancellationToken = default)
            where TRequest : class, new()
        {
            using var message = new HttpRequestMessage(method, endpoint);

            if (request != null)
            {
                message.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            }

            using var response = await _client.SendAsync(message, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<TResponse>(content);
        }

        public async Task<TResponse> RequestAsync<TResponse>(
            HttpMethod method,
            string endpoint,
            CancellationToken cancellationToken = default)
        {
            using var message = new HttpRequestMessage(method, endpoint);
            using var response = await _client.SendAsync(message, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<TResponse>(content);
        }

        public async Task<Stream> RequestStreamAsync(
            HttpMethod method,
            string endpoint,
            CancellationToken cancellationToken = default)
        {
            using var message = new HttpRequestMessage(method, endpoint);
            using var response = await _client.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var ret = new MemoryStream(new byte[stream.Length]);
            await stream.CopyToAsync(ret);
            ret.Position = 0;
            return ret;
        }

        public async Task<Stream> RequestStreamAsync<TRequest>(
            HttpMethod method,
            TRequest request,
            string endpoint,
            CancellationToken cancellationToken = default)
        {
            using var message = new HttpRequestMessage(method, endpoint);

            if (request != null)
            {
                message.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            }

            using var response = await _client.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var ret = new MemoryStream(new byte[stream.Length]);
            await stream.CopyToAsync(ret);
            ret.Position = 0;
            return ret;
        }
        #endregion

        #region Useless (Design-time APIs)
        public Task<Guid> CreateWorkflowAsync(CreateWorkflowRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> CreateWorkflowInstanceAsync(Guid id, int version, Dictionary<string, JToken> arguments, CancellationToken cancellationToken = default)
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

        public Task<int?> GetLatestVersionAsync(Guid workflowId, WorkflowVersionStatus? status = WorkflowVersionStatus.Available, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<GetWorkflowInstanceResult>> GetWorkflowInstancesAsync(Guid workflowId, int? version, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Pomelo.Workflow.Models.Workflow>> GetWorkflowsAsync(string name = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateWorkflowAsync(Guid workflowId, UpdateWorkflowRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<GetWorkflowVersionResult>> GetWorkflowVersionsAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateWorkflowVersionStatusAsync(Guid workflowId, int version, WorkflowVersionStatus status, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        #endregion

        public Task CreateWorkflowInstanceConnectionAsync(
            WorkflowInstanceConnection request, 
            CancellationToken cancellationToken = default)
            => RequestAsync<WorkflowInstanceConnection, ApiResult>(
                HttpMethod.Post,
                $"/api/project/{_context.ProjectId}/pipeline/${_context.PipelineId}/job/{_context.JobNumber}/diagram-stage/{_context.WorkflowInstanceId}/connection",
                request,
                null,
                cancellationToken);

        public Task<Guid> CreateWorkflowStepAsync(Guid instanceId, WorkflowInstanceStep step, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WorkflowInstanceStep>> GetInstanceStepsAsync(Guid instanceId, CancellationToken cancellationToken = default)
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

        public Task<WorkflowInstanceStep> GetWorkflowInstanceStepAsync(Guid stepId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<WorkflowVersion> GetWorkflowVersionAsync(Guid workflowId, int version, CancellationToken cancellationToken)
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

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
