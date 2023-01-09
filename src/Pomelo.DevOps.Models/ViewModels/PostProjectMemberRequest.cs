using System;

namespace Pomelo.DevOps.Models.ViewModels
{
    public class PostProjectMemberRequest
    {
        public Guid UserId { get; set; }

        public ProjectMemberRole Role { get; set; }
    }
}
