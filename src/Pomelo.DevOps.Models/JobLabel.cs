using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Pomelo.DevOps.Models
{
    public class JobLabel
    {
        [ForeignKey(nameof(Job))]
        public Guid JobId { get; set; }

        [JsonIgnore]
        public virtual Job Job { get; set; }

        [MaxLength(32)]
        public string Label { get; set; }
    }
}
