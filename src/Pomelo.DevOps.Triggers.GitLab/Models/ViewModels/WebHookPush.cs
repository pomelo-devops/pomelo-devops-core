// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Pomelo.DevOps.Triggers.GitLab.Models.ViewModels
{
    public class WebHookPush : WebHookBase
    {
        [JsonProperty(PropertyName = "event_name")]
        public string EventName { get; set; }

        [JsonProperty(PropertyName = "before")]
        public string Before { get; set; }

        [JsonProperty(PropertyName = "after")]
        public string After { get; set; }

        [JsonProperty(PropertyName = "ref")]
        public string Ref { get; set; }

        [JsonProperty(PropertyName = "checkout_sha")]
        public string CheckoutSha { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public long UserId { get; set; }

        [JsonProperty(PropertyName = "user_name")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "user_email")]
        public string UserEmail { get; set; }

        [JsonProperty(PropertyName = "user_avatar")]
        public string UserAvatar { get; set; }

        [JsonProperty(PropertyName = "project_id")]
        public long ProjectId { get; set; }

        [JsonProperty(PropertyName = "project")]
        public WebHookProject Project { get; set; }

        [JsonProperty(PropertyName = "commits")]
        public IEnumerable<WebHookCommit> Commits { get; set; }

        [JsonProperty(PropertyName = "total_commits_count")]
        public long TotalCommitsCount { get; set; }
    }
}
