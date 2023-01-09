using System;
using System.Collections.Generic;

namespace Pomelo.DevOps.Models.ViewModels
{
    public class ExecuteJobRequest
    {
        public Guid AgentId { get; set; }

        public bool IsParallel { get; set; } = false;

        public int? Identifier { get; set; }

        public IEnumerable<Guid> ProhibitStages { get; set; } = new List<Guid>();
    }
}
