// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Triggers.GitLab.Models
{
    public enum TriggerType
    {
        Push,
        MergeRequest
    }

    public class Trigger
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        [MaxLength(64)]
        public string GitLabNamespace { get; set; }

        [MaxLength(64)]
        public string GitLabProject { get; set; }

        [MaxLength(64)]
        public string PomeloDevOpsProject { get; set; }

        [MaxLength(64)]
        public string PomeloDevOpsPipeline { get; set; }

        public TriggerType Type { get; set; }

        public string ArgumentsJson { get; set; } = "{}";

        public bool Enabled { get; set; }

        public string Branch { get; set; } // Only for push

        public string JobNameTemplate { get; set; }

        public string JobDescriptionTemplate { get; set; }

        [NotMapped]
        public IDictionary<string, string> Arguments => JsonConvert.DeserializeObject<IDictionary<string, string>>(ArgumentsJson);
    }
}
