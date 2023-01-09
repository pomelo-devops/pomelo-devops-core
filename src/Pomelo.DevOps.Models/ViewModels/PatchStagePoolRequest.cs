using System;

namespace Pomelo.DevOps.Models.ViewModels
{
    public class PatchStagePoolRequest
    {
        public Guid JobId { get; set; }

        public string StageName { get; set; }

        public long AgentPool { get; set; }
    }
}
