using System.Collections.Generic;

namespace Pomelo.DevOps.Models
{
    public class DashboardItem
    {
        public string WidgetId { get; set; }

        public Dictionary<string, object> Arguments { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }

    public class Dashboard
    {
        public List<DashboardItem> Items { get; set; } = new List<DashboardItem>();
    }
}
