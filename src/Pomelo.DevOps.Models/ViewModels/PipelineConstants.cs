using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Models.ViewModels
{
    public enum PipelineConstantsFieldType
    {
        Plain,
        Secret
    }

    public class PipelineConstants
    {
        [YamlMember(Alias = "key")]
        public string Key { get; set; }

        [YamlMember(Alias = "value")]
        public string Value { get; set; }

        [YamlMember(Alias = "type")]
        public PipelineConstantsFieldType Type { get; set; }
    }
}
