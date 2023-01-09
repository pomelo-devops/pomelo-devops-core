// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Pomelo.DevOps.Triggers.GitLab.Models.ViewModels
{
    public class WebHookMergeRequestUser
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }

    public class WebHookMergeRequestRepository
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "homepage")]
        public string Homepage { get; set; }
    }

    public class WebHookMergeRequestObjectAttributes
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "target_branch")]
        public string TargetBranch { get; set; }

        [JsonProperty(PropertyName = "source_branch")]
        public string SourceBranch { get; set; }

        [JsonProperty(PropertyName = "source_project_id")]
        public int SourceProjectId { get; set; }

        [JsonProperty(PropertyName = "author_id")]
        public int? AuthorId { get; set; }

        [JsonProperty(PropertyName = "assignee_id")]
        public int? AssigneeId { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty(PropertyName = "updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "milestone_id")]
        public int? MilestoneId { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "merge_status")]
        public string MergeStatus { get; set; }

        [JsonProperty(PropertyName = "target_project_id")]
        public int? TargetProjectId { get; set; }

        [JsonProperty(PropertyName = "iid")]
        public int? Iid { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "updated_by_id")]
        public int? UpdatedById { get; set; }

        [JsonProperty(PropertyName = "merge_error")]
        public string MergeError { get; set; }

        [JsonProperty(PropertyName = "merge_params")]
        public IDictionary<string, object> MergeParams { get; set; }

        [JsonProperty(PropertyName = "merge_when_pipeline_succeeds")]
        public bool MergeWhenPipelineSucceeds { get; set; }

        [JsonProperty(PropertyName = "merge_user_id")]
        public int? MergeUserId { get; set; }

        [JsonProperty(PropertyName = "merge_commit_sha")]
        public string MergeCommitSha { get; set; }

        [JsonProperty(PropertyName = "deleted_at")]
        public string? DeletedAt { get; set; }

        [JsonProperty(PropertyName = "in_progress_merge_commit_sha")]
        public string InProgressMergeCommitSha { get; set; }

        [JsonProperty(PropertyName = "lock_version")]
        public int? LockVersion { get; set; }

        [JsonProperty(PropertyName = "time_estimate")]
        public int? TimeEstimate { get; set; }

        [JsonProperty(PropertyName = "last_edited_at")]
        public string? LastEditedAt { get; set; }

        [JsonProperty(PropertyName = "last_edited_by_id")]
        public int? LastEditedById { get; set; }

        [JsonProperty(PropertyName = "head_pipeline_id")]
        public int? HeadPipelineId { get; set; }

        [JsonProperty(PropertyName = "ref_fetched")]
        public bool? RefFetched { get; set; }

        [JsonProperty(PropertyName = "merge_jid")]
        public int? MergeJid { get; set; }

        [JsonProperty(PropertyName = "source")]
        public WebHookProject Source { get; set; }

        [JsonProperty(PropertyName = "target")]
        public WebHookProject Target { get; set; }

        [JsonProperty(PropertyName = "last_commit")]
        public WebHookCommit LastCommit { get; set; }

        [JsonProperty(PropertyName = "work_in_progress")]
        public bool? WorkInProgress { get; set; }

        [JsonProperty(PropertyName = "total_time_spent")]
        public int? TotalTimeSpent { get; set; }

        [JsonProperty(PropertyName = "human_total_time_spent")]
        public int? HumanTotalTimeSpent { get; set; }

        [JsonProperty(PropertyName = "human_time_estimate")]
        public int? HumanTimeEstimate { get; set; }

        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }
    }

    public class WebHookMergeRequest : WebHookBase
    {
        [JsonProperty(PropertyName = "user")]
        public WebHookMergeRequestUser User { get; set; }

        [JsonProperty(PropertyName = "project")]
        public WebHookProject Project { get; set; }

        [JsonProperty(PropertyName = "object_attributes")]
        public WebHookMergeRequestObjectAttributes ObjectAttributes { get; set; }

        [JsonProperty(PropertyName = "labels")]
        public object Labels { get; set; }

        [JsonProperty(PropertyName = "repository")]
        public WebHookMergeRequestRepository Repository { get; set; }
    }
}
