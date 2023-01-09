using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public class Project
    {
        [MaxLength(64)]
        public string Id { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        [ForeignKey(nameof(Icon))]
        public Guid? IconId { get; set; }

        public virtual Binary Icon { get; set; }

        [MaxLength(128)]
        public string ExtensionPath { get; set; }

        [Column(TypeName = "json")]
        public Dashboard Dashboard { get; set; } = new Dashboard();

        public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    }
}
