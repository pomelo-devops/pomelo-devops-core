using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Models.ViewModels
{
    public enum RunCondition
    {
        CheckVariable,
        RequirePreviousTaskSuccess,
        RunAnyway,
        RequirePreviousTaskFailed
    }

    public class PipelineStageStep
    {
        [MaxLength(64)]
        [YamlMember(Alias = "name", Order = 1)]
        public string Name { get; set; }

        [YamlMember(Alias = "order", Order = 2)]
        public int Order { get; set; }

        [MaxLength(64)]
        [YamlMember(Alias = "step", Order = 3)]
        public string StepId { get; set; }

        [MaxLength(64)]
        [YamlMember(Alias = "version", Order = 4)]
        public string Version { get; set; }

        [YamlMember(Alias = "method", Order = 5)]
        [MaxLength(64)]
        public string Method { get; set; }

        [NotMapped]
        [YamlMember(Alias = "arguments", Order = 6)]
        public Dictionary<string, string> Arguments { get; set; }

        [YamlMember(Alias = "condition", Order = 7)]
        public RunCondition Condition { get; set; }

        [MaxLength(64)]
        [YamlMember(Alias = "condition_variable", Order = 8, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string ConditionVariable { get; set; }

        [YamlMember(Alias = "timeout", Order = 9)]
        public int Timeout { get; set; } = -1;

        [YamlMember(Alias = "retry", Order = 10, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public int Retry { get; set; } = 0;

        [YamlMember(Alias = "error_handling_mode", Order = 11, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public ErrorHandlingMode ErrorHandlingMode { get; set; }
    }
}
