using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public enum PipelineAccessType
    {
        Reader,
        Collaborator,
        Master
    }

    public class PipelineAccess
    {
        [Key]
        [MaxLength(64)]
        [ForeignKey(nameof(Pipeline))]
        public string PipelineId { get; set; }

        public virtual Pipeline Pipeline { get; set; }

        [Key]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }

        public PipelineAccessType AccessType { get; set; }
    }
}
