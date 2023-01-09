using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pomelo.DevOps.Models
{
    public enum LoginProviderType
    { 
        Local,
        OAuth2,
        Ldap,
        Ext1,
        Ext2,
        Ext3,
        Ext4,
        Ext5
    }

    public record class LoginProvider
    {
        [MaxLength(64)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }

        [MaxLength(64)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name { get; set; }

        public LoginProviderType Mode { get; set; }

        public bool Enabled { get; set; }

        [JsonIgnore]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Body { get; set; }

        [MaxLength(256)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string IconUrl { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Priority { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ClientSecret { get; set; }

        [JsonIgnore]
        public virtual ICollection<User> Users { get; set; }
    }
}
