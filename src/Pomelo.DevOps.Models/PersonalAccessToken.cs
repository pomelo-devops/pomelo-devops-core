using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public class PersonalAccessToken
    {
        public Guid Id { get; set; }

        [MaxLength(64)]
        public string Token { get; set; }

        public string Name { get; set; }
        
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpireAt { get; set; } = null;
    }
}
