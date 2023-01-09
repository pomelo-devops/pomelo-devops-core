using System.Collections.Generic;

namespace Pomelo.DevOps.Models.ViewModels
{
    public class PostLogsRequest
    {
        public IEnumerable<Log> Logs { get; set; }
    }
}
