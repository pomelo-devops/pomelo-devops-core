using System;
using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Models.ViewModels
{
    public record GetPipelineResponse : PutPipelineRequest
    {
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string Id { get; set; }

        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public Guid UserId { get; set; }

        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string ProjectId { get; set; }
    }
}
