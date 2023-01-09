using System.Collections.Generic;

namespace Pomelo.DevOps.Models.ViewModels
{
    public class PostPipelineJobRequest
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public PipelineJobTriggerType TriggerType { get; set; }

        public string TriggerName { get; set; }

        public Dictionary<string, string> Arguments { get; set; }
    }
}
