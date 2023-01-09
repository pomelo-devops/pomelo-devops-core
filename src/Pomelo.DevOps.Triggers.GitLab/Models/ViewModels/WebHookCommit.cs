// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Pomelo.DevOps.Triggers.GitLab.Models.ViewModels
{

    public class WebHookCommitAuthor
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }

    public class WebHookCommit
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "author")]
        public WebHookCommitAuthor Author { get; set; }
    }
}
