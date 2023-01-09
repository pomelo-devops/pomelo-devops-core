using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Models.ViewModels
{
    public enum IsolationLevel
    {
        Parallel,
        Sequential
    }

    public class PipelineStage
    {
        [MaxLength(64)]
        [YamlMember(Alias = "name", Order = 1)]
        public string Name { get; set; }

        [YamlMember(Alias = "order", Order = 2)]
        public int Order { get; set; }

        [YamlMember(Alias = "pool", Order = 3)]
        public long? AgentPoolId { get; set; }

        [YamlMember(Alias = "isolation_level", Order = 4)]
        public IsolationLevel IsolationLevel { get; set; }

        [YamlMember(Alias = "agent_count", Order = 5)]
        public int AgentCount { get; set; }

        [YamlMember(Alias = "condition", Order = 6)]
        public RunCondition Condition { get; set; }

        [MaxLength(64)]
        [YamlMember(Alias = "condition_variable", Order = 7, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string ConditionVariable { get; set; }

        [YamlMember(Alias = "timeout", Order = 8)]
        public int Timeout { get; set; } = -1;

        [YamlMember(Alias = "steps", Order = 9)]
        public List<PipelineStageStep> Steps { get; set; }
    }
}
