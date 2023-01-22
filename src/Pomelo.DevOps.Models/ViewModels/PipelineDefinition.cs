using System;
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
        [YamlMember(Alias = "type", Order = 0, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public PipelineType Type { get; set; }

        [YamlMember(Alias = "stages", Order = 1, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public List<PipelineStage> Stages { get; set; } = new List<PipelineStage>();

        [YamlMember(Alias = "pipeline_workflow_id", Order = 1, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public Guid? PipelineWorkflowId { get; set; }

        [YamlMember(Alias = "arguments", Order = 2)]
        public List<PipelineArgument> Arguments { get; set; } = new List<PipelineArgument>();

        [YamlMember(Alias = "constants", Order = 3)]
        public List<PipelineConstants> Constants { get; set; } = new List<PipelineConstants>();

        [YamlMember(Alias = "timeout", Order = 4)]
        public int Timeout { get; set; } = -1;
    }
}
