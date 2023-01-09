using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public enum LogLevel
    { 
        Information,
        Warning,
        Error
    }

    public class Log
    {
        public Guid Id { get; set; }

        [MaxLength(64)]
        public string PipelineId { get; set; }

        public long JobNumber { get; set; }

        [MaxLength(64)]
        public string LogSet { get; set; }

        public DateTime Time { get; set; } = DateTime.UtcNow;

        public LogLevel Level { get; set; }

        public string Text { get; set; }
    }
}
