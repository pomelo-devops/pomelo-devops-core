using System.Collections.Generic;

namespace Pomelo.DevOps.Models.ViewModels
{
    public class GetWidgetEntriesRequest
    {
        public IEnumerable<string> WidgetIds { get; set; }
    }
}
