using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public class Addin
    {
        [MaxLength(64)]
        public string Id { get; set; }

        [MaxLength(256)]
        public string Path { get; set; }

        public string Content { get; set; }

        [MaxLength(64)]
        [ForeignKey(nameof(Organization))]
        public string OrganizationId { get; set; }

        public virtual Project Organization { get; set; }
    }
}
