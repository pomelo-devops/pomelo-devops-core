using Pomelo.DevOps.Models;
using System;

namespace Pomelo.DevOps.Models.ViewModels
{
    public class Log
    {
        public DateTime Time { get; set; }

        public LogLevel Level { get; set; }

        public string Text { get; set; }
    }
}
