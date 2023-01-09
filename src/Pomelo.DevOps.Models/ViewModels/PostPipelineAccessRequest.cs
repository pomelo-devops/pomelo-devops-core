using System;

namespace Pomelo.DevOps.Models.ViewModels
{
    public class PostPipelineAccessRequest
    {
        public Guid UserId { get; set; }

        public PipelineAccessType AccessType { get; set; }
    }
}
