// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Models;
using System.Linq;

namespace Pomelo.DevOps.Agent
{
    public class VariableContainer
    {
        public static readonly Regex VariableRegex = new Regex(@"(?<=\$\()[a-zA-Z0-9-_]{1,}(?=\))");
        private ConcurrentDictionary<string, string> _dic;
        private Connector _connector;
        private ConfigManager _config;
        private Guid _stageId;
        private string _pipelineId;
        private Guid _jobId;
        private long _jobNumber;

        protected ConcurrentDictionary<string, string> Variables => _dic;
        public ICollection<string> Keys => _dic.Keys;

        public VariableContainer(
            Connector connector,
            ConfigManager config, 
            Guid stageId, 
            string pipelineId, 
            Guid jobId, 
            long jobNumber)
        {
            _connector = connector;
            _config = config;
            _stageId = stageId;
            _jobId = jobId;
            _pipelineId = pipelineId;
            _jobNumber = jobNumber;
            _dic = new ConcurrentDictionary<string, string>();
            _dic["JOB_ID"] = _jobId.ToString();
            _dic["JOB_NUMBER"] = _jobNumber.ToString();
            _dic["STAGE_ID"] = _stageId.ToString();
            _dic["PIPELINE_SERVER"] = _config.Config.Server;
            _dic["AGENT_IDENTIFIER"] = _config.Config.Identifier.ToString();
            _dic["PIPELINE_PROJECT"] = _config.Config.ProjectId;
            _dic["PIPELINE_ID"] = _pipelineId;
            _dic["AGENT_API_HOST"] = "http://localhost:5500";
            _dic["AGENT_API_JOB"] = $"/api/project/{_config.Config.ProjectId}/pipeline/{pipelineId}/job/{_jobNumber}";
        }

        public async ValueTask LoadVariablesAsync(JobStage job, CancellationToken cancellationToken = default)
        {
            Clear();
            _dic["PIPELINE_SERVER"] = _config.Config.Server;
            _dic["STAGE_ID"] = job.Id.ToString();
            _dic["JOB_ID"] = job.PipelineJobId.ToString();
            _dic["JOB_NUMBER"] = job.JobNumber.ToString();
            _dic["AGENT_IDENTIFIER"] = _config.Config.Identifier.ToString();
            _dic["PIPELINE_PROJECT"] = _config.Config.ProjectId;
            _dic["PIPELINE_ID"] = job.Pipeline;
            _dic["AGENT_API_HOST"] = "http://localhost:5500";
            _dic["AGENT_API_JOB"] = $"/api/project/{_config.Config.ProjectId}/pipeline/{job.Pipeline}/job/{job.JobNumber}";
            var hashmap = await _connector.GetConstantAsync(job, cancellationToken);
            foreach(var x in hashmap)
            {
                _dic[x.Key] = x.Value;
            }
            var variables = await _connector.GetJobVariablesAsync(job, cancellationToken);
            foreach (var x in variables)
            {
                _dic[x.Name] = x.Value;
            }
        }

        public void PutBatchVariables(IDictionary<string, string> variables)
        {
            foreach(var x in variables)
            {
                _dic[x.Key] = x.Value;
            }
        }

        public void PutVariable(string name, string value)
        {
            _dic[name] = value;
        }

        public string GetVariable(string name, int depth = 0)
        {
            if (depth == 100)
            {
                return "";
            }

            if (!_dic.ContainsKey(name))
            {
                return "";
            }

            return EscapeString(_dic[name], depth + 1);
        }

        public string EscapeString(string source, int depth = 0)
        {
            if (source == null)
            {
                return "";
            }

            var ret = source;
            var matched = VariableRegex.Matches(ret).Cast<Match>();
            foreach (var x in matched)
            {
                var val = GetVariable(x.Value, depth);
                ret = ret.Replace("$(" + x.Value + ")", val ?? string.Empty);
            }
            return ret;
        }

        public void Clear()
        {
            _dic.Clear();
        }
    }
}
