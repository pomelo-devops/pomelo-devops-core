using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public class JobExtension
    {
        [MaxLength(64)]
        public string Id { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        public string Description { get; set; }

        [MaxLength(256)]
        public string ExtensionEntryUrl { get; set; }

        public string Token { get; set; }

        [MaxLength(256)]
        public string IconUrl { get; set; }

        [MaxLength(256)]
        public string ViewUrl { get; set; }

        [MaxLength(256)]
        public string ManageUrl { get; set; }

        public int Priority { get; set; }

        public virtual ICollection<PipelineTrigger> PipelineTriggers { get; set; }

        [NotMapped]
        public string ExtensionOriginalBaseUrl
            => ExtensionEntryUrl.Substring(0, ExtensionEntryUrl.IndexOf("/", ExtensionEntryUrl.IndexOf("//") + 2));

        [NotMapped]
        public string ProviderProxiedBaseUrl
            => $"/api/jobextension/{Id}";

        [NotMapped]
        public string ProxiedIconUrl
            => ProviderProxiedBaseUrl + IconUrl;

        [NotMapped]
        public string ProxiedManageUrl
            => ProviderProxiedBaseUrl + ManageUrl;
    }
}
