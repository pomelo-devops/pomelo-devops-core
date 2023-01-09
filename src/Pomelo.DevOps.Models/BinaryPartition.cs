using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public class BinaryPartition
    {
        [Key]
        [ForeignKey(nameof(Binary))]
        public Guid BinaryId { get; set; }

        public virtual Binary Binary { get; set; }

        [Key]
        public int Partition { get; set; }

        public byte[] Bytes { get; set; }
    }
}
  