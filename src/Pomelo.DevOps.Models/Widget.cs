using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Pomelo.DevOps.Models
{
    public enum WidgetPackageItemType
    {
        File,
        Directory
    }

    public class WidgetPackageItem
    {
        public string Name { get; set; }

        public WidgetPackageItemType Type { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public byte[] Content { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<WidgetPackageItem> Children { get; set; } = new List<WidgetPackageItem>();
    }

    public record WidgetBase
    {
        [MaxLength(64)]
        public string Id { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        public string Description { get; set; }

        [MaxLength(256)]
        public string IconUrl { get; set; }

        [MaxLength(256)]
        public string WidgetEntryView { get; set; }

        [MaxLength(256)]
        public string ConfigEntryView { get; set; }
    }

    public record Widget : WidgetBase
    {
        [Column(TypeName = "json")]
        public List<WidgetPackageItem> Items { get; set; }
    }
}
