// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Agent
{
    public class Connector : IDisposable
    {
        private HttpClient _client;
        private ConfigManager _config;
        private IDeserializer yamlDeserializer;
        private static readonly string AssemblyDirectory 
            = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string BuildVersionPath
            = Path.Combine(AssemblyDirectory, "build.txt");
        private static readonly string CilentVersion = File.Exists(BuildVersionPath)
            ? File.ReadAllText(BuildVersionPath).Trim()
            : null;

        protected virtual IDeserializer YamlDeserializer
        {
            get
            {
                if (yamlDeserializer == null)
                {
                    yamlDeserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                }
                return yamlDeserializer;
            }
        }

        public Connector(ConfigManager config)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            _config = config;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Authorization", "Token " + _config.Config.PersonalAccessToken);
            if (CilentVersion != null)
            {
                _client.DefaultRequestHeaders.Add("X-Pomelo-Agent-Version", CilentVersion);
            }

        }

        public void RefreshHeaders()
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Add("Authorization", "Token " + _config.Config.PersonalAccessToken);
        }

        public async ValueTask<Pomelo.DevOps.Models.Agent> RegisterAgentAsync(CancellationToken cancellationToken = default)
        {
            var result = await PostApiAsync<Pomelo.DevOps.Models.Agent>($"/api/project/{_config.Config.ProjectId}/agentpool/{_config.Config.AgentPoolId}/agent", new
            {
                Status = AgentStatus.Idle,
                HeartBeat = DateTime.UtcNow.ToString()
            }, cancellationToken);
            if (result.Code != 200)
            {
                throw new ConnectorException(result.Code, result.Message);
            }
            _config.Config.AgentId = result.Data.Id;
            _config.Save();
            return result.Data;
        }

        public async ValueTask<string> GetWaitingJobAsync(IEnumerable<Guid> prohibitStages, IsolationLevel mode, CancellationToken cancellationToken = default)
        {
            var identifier = _config.Config.Identifier;
            var agentId = _config.Config.AgentId ?? default;
            var server = _config.Config.Server.TrimEnd('/');

            using (var content = new StringContent(JsonConvert.SerializeObject(new ExecuteJobRequest
            {
                AgentId = agentId,
                Identifier = identifier == null ? null : (int?)Convert.ToInt32(identifier),
                IsParallel = mode == IsolationLevel.Parallel,
                ProhibitStages = prohibitStages.ToList()
            }), Encoding.UTF8, "application/json"))
            using (var response = await _client.PostAsync(server + "/api/misc/execute-job", content, cancellationToken))
            {
                var responseText = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.PreconditionFailed) 
                {
                    Console.Error.WriteLine("Agent version has not been accepted by server.");
                    Environment.Exit(412);
                    return null;
                }
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ConnectorException((int)response.StatusCode, responseText);
                }
                return responseText;
            }
        }

        public JobStage ParseJobStage(string yml)
        {
            return yamlDeserializer.Deserialize<JobStage>(yml);
        }

        public async ValueTask UpdateStepStatusAsync(JobStage stage, JobStep step, CancellationToken cancellationToken = default)
        {
            var result = await PostApiAsync<JobStep>($"/api/project/{stage.Project}/pipeline/{stage.Pipeline}/job/{stage.JobNumber}/stage/{stage.Id}/step/{step.Id}", step, cancellationToken);
            if (result.Code != 200)
            {
                throw new ConnectorException(result.Code, result.Message);
            }
            return;
        }

        public async ValueTask UpdateStageStatusAsync(JobStage stage, CancellationToken cancellationToken = default)
        {
            var result = await PostApiAsync<JobStep>($"/api/project/{stage.Project}/pipeline/{stage.Pipeline}/job/{stage.JobNumber}/stage/{stage.Id}", stage, cancellationToken);
            if (result.Code != 200)
            {
                throw new ConnectorException(result.Code, result.Message);
            }
            return;
        }

        public async ValueTask<List<JobVariable>> GetJobVariablesAsync(JobStage job, CancellationToken cancellationToken = default)
        {
            var result = await GetApiAsync<List<JobVariable>>($"/api/project/{job.Project}/pipeline/{job.Pipeline}/job/{job.JobNumber}/variable", cancellationToken);
            if (result.Code != 200)
            {
                throw new ConnectorException(result.Code, result.Message);
            }
            return result.Data;
        }

        public async ValueTask SetJobVariableAsync(JobStage job, string name, string value, CancellationToken cancellationToken = default)
        {
            var result = await PostApiAsync<JobVariable>($"/api/project/{job.Project}/pipeline/{job.Pipeline}/job/{job.JobNumber}/variable/{name}", new JobVariable
            {
                Name = name,
                Value = value,
                PipelineJobId = job.Id
            }, cancellationToken);
            if (result.Code != 200)
            {
                throw new ConnectorException(result.Code, result.Message);
            }
            return;
        }

        public async ValueTask LogAsync(JobStage job, string logSet, IEnumerable<Pomelo.DevOps.Models.ViewModels.Log> logs, CancellationToken cancellationToken = default)
        {
            var result = await PostApiAsync<JobVariable>($"/api/project/{job.Project}/pipeline/{job.Pipeline}/job/{job.JobNumber}/log/{logSet}", new PostLogsRequest
            {
                Logs = logs
            }, cancellationToken);
            if (result.Code != 200)
            {
                throw new ConnectorException(result.Code, result.Message);
            }
            return;
        }

        public async ValueTask PostMetricsAsync(string projectId, long pool, Metrics metrics, CancellationToken cancellationToken = default)
        {
            if (metrics == null)
            { 
                return; 
            }

            await PostApiAsync<Metrics>($"/api/project/{projectId}/agentpool/{pool}/agent/{metrics.AgentId}/metrics", metrics, cancellationToken);
        }

        public async ValueTask<IEnumerable<PipelineConstants>> GetConstantAsync(JobStage job, CancellationToken cancellationToken = default)
        {
            var result = await GetApiAsync<IEnumerable<PipelineConstants>>($"/api/project/{job.Project}/pipeline/{job.Pipeline}/constant", cancellationToken);
            if (result.Code != 200)
            {
                throw new ConnectorException(result.Code, result.Message);
            }
            return result.Data;
        }

        public async ValueTask<ApiResult<T>> PostApiAsync<T>(string endpoint, object request, CancellationToken cancellationToken = default)
        {
            var result = await PostApiPlainAsync(endpoint, JsonConvert.SerializeObject(request), cancellationToken);
            return JsonConvert.DeserializeObject<ApiResult<T>>(result.Content);
        }

        public async ValueTask<(string Content, int StatusCode)> PostApiPlainAsync(string endpoint, string request, CancellationToken cancellationToken = default)
        {
            var server = _config.Config.Server.TrimEnd('/');
            using (var content = new StringContent(request, Encoding.UTF8, "application/json"))
            using (var response = await _client.PostAsync(server + endpoint, content, cancellationToken))
            {
                var responseText = await response.Content.ReadAsStringAsync();
                return (responseText, (int)response.StatusCode);
            }
        }

        public async ValueTask<(string Content, int StatusCode)> DeleteApiPlainAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            var server = _config.Config.Server.TrimEnd('/');
            using (var response = await _client.DeleteAsync(server + endpoint, cancellationToken))
            {
                var responseText = await response.Content.ReadAsStringAsync();
                return (responseText, (int)response.StatusCode);
            }
        }

        public async ValueTask<ApiResult<T>> GetApiAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        {
            var result = await GetApiPlainAsync(endpoint, cancellationToken);
            return JsonConvert.DeserializeObject<ApiResult<T>>(result.Content);
        }

        public async ValueTask<(string Content, int StatusCode)> GetApiPlainAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            var server = _config.Config.Server.TrimEnd('/');
            using (var response = await _client.GetAsync(server + endpoint, cancellationToken))
            {
                var responseText = await response.Content.ReadAsStringAsync();
                return (responseText, (int)response.StatusCode);
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
