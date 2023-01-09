using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public enum ProjectMemberRole
    {
        Member,
        Agent,
        Admin
    }

    public class ProjectMember
    {
        [Key]
        [MaxLength(64)]
        [ForeignKey(nameof(Project))]
        public string ProjectId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Project Project { get; set; }

        [Key]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual User User { get; set; }

        public ProjectMemberRole Role { get; set; }
    }
}
