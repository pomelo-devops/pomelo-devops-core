// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Agent.Models
{
    public class AdhocStep
    {
        [YamlMember(Alias = "id")]
        public Guid Id { get; set; }

        [YamlMember(Alias = "step")]
        public string Step { get; set; }

        [YamlMember(Alias = "version")]
        public string Version { get; set; }

        [YamlMember(Alias = "feed")]
        public string Feed { get; set; }

        [YamlMember(Alias = "method")]
        public string Method { get; set; }

        [YamlMember(Alias = "started_at")]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        [YamlMember(Alias = "finished_at")]
        public DateTime? FinishedAt { get; set; }

        [YamlMember(Alias = "arguments", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public IDictionary<string, string> Arguments { get; set; }

        [YamlIgnore]
        public int ExitCode { get; set; }

        [YamlIgnore]
        public string NewOutput { get; set; } = "";

        [YamlIgnore]
        public string NewError { get; set; } = "";

        [YamlIgnore]
        public string OutputFull { get; set; } = "";

        [YamlIgnore]
        public string ErrorFull { get; set; } = "";

        [YamlMember(Alias = "timeout")]
        public int Timeout { get; set; } = -1;
    }
}
