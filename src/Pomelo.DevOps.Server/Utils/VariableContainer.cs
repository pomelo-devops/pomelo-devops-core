// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Linq;

namespace Pomelo.DevOps.Server.Utils
{
    public class VariableContainer
    {
        public static readonly Regex VariableRegex = new Regex(@"(?<=\$\()[a-zA-Z0-9-_]{1,}(?=\))");
        private ConcurrentDictionary<string, string> _dic;
        private string _pipelineId;
        private Guid _jobId;

        protected ConcurrentDictionary<string, string> Variables => _dic;
        public ICollection<string> Keys => _dic.Keys;

        public VariableContainer(Guid jobId, string pipelineId, string organizationId)
        {
            _jobId = jobId;
            _pipelineId = pipelineId;
            _dic = new ConcurrentDictionary<string, string>();
            _dic["JOB_ID"] = _jobId.ToString();
            _dic["PIPELINE_ORGNIZATION"] = organizationId;
            _dic["PIPELINE_ID"] = _pipelineId;
        }

        public void PutBatchVariables(IDictionary<string, string> variables)
        {
            foreach (var x in variables)
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
