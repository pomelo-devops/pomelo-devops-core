using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public class UserSession
    {
        [MaxLength(64)]
        public string Id { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddMinutes(20);

        [MaxLength(512)]
        public string Agent { get; set; }

        [MaxLength(64)]
        public string Address { get; set; }
    }
}
