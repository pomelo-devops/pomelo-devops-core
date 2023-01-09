using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pomelo.DevOps.Models
{
    public class Binary
    {
        public Guid Id { get; set; }

        public long Size { get; set; }

        public string Filename { get; set; }

        [MaxLength(64)]
        public string Sha256 { get; set; }

        [MaxLength(32)]
        public string ContentType { get; set; }

        public virtual ICollection<BinaryPartition> Partitions { get; set; }
    }
}
