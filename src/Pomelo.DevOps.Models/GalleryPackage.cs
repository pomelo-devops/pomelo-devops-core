using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    [Flags]
    public enum Platform
    {
        Windows = 0x01,
        Linux = 0x02,
        Mac = 0x04
    }

    public class GalleryPackage
    {
        [MaxLength(64)]
        public string Id { get; set; }

        [ForeignKey(nameof(Icon))]
        public Guid? IconId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual Binary Icon { get; set; }

        [MaxLength(64)]
        public string Version { get; set; }

        [ForeignKey(nameof(Author))]
        public Guid AuthorId { get; set; }

        public virtual User Author { get; set; }

        public bool ListInGallery { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        public string Description { get; set; }

        [ForeignKey(nameof(Package))]
        public Guid PackageBinaryId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual Binary Package { get; set; }

        public string Methods { get; set; }

        public Platform Platform { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public long Downloads { get; set; } = 0;
    }
}
