using System.Xml.Linq;
using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Models.ViewModels
{
    public record PutPipelineRequest : PipelineDefinition
    {
        [YamlIgnore]
        public string Name { get; set; }

        [YamlIgnore]
        public PipelineVisibility Visibility { get; set; }
    }
}
