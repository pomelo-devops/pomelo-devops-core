// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Pomelo.DevOps.Triggers.GitLab.Models.ViewModels
{

    public class WebHookProject
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "web_url")]
        public string WebUrl { get; set; }

        [JsonProperty(PropertyName = "avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty(PropertyName = "git_ssh_url")]
        public string GitSshUrl { get; set; }

        [JsonProperty(PropertyName = "git_http_url")]
        public string GitHttpUrl { get; set; }

        [JsonProperty(PropertyName = "namespace")]
        public string Namespace { get; set; }

        [JsonProperty(PropertyName = "visibility_level")]
        public int VisibilityLevel { get; set; }

        [JsonProperty(PropertyName = "path_with_namespace")]
        public string PathWithNamespace { get; set; }

        [JsonProperty(PropertyName = "default_branch")]
        public string DefaultBranch { get; set; }

        [JsonProperty(PropertyName = "ci_config_path")]
        public string CiCOnfigPath { get; set; }

        [JsonProperty(PropertyName = "homepage")]
        public string Homepage { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "ssh_url")]
        public string SshUrl { get; set; }

        [JsonProperty(PropertyName = "http_url")]
        public string HttpUrl { get; set; }
    }
}
