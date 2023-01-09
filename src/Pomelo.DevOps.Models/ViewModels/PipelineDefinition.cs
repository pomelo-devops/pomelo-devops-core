using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Models.ViewModels
{
    public enum PipelineVisibility
    {
        Private,
        Public
    }

    public enum ErrorHandlingMode
    {
        Normal,
        IgnoreFail,
        StderrAsFail
    }

    public record PipelineDefinition
    {
        [YamlMember(Alias = "stages", Order = 1)]
        public List<PipelineStage> Stages { get; set; } = new List<PipelineStage>();

        [YamlMember(Alias = "arguments", Order = 2)]
        public List<PipelineArgument> Arguments { get; set; } = new List<PipelineArgument>();

        [YamlMember(Alias = "constants", Order = 3)]
        public List<PipelineConstants> Constants { get; set; } = new List<PipelineConstants>();

        [YamlMember(Alias = "timeout", Order = 4)]
        public int Timeout { get; set; } = -1;
    }
}
