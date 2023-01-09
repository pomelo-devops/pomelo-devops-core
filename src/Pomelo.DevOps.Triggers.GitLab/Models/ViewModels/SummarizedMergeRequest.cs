// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

namespace Pomelo.DevOps.Triggers.GitLab.Models.ViewModels
{
    public class SummarizedMergeRequest
    {
        public SummarizedMergeRequest() { }

        public SummarizedMergeRequest(WebHookMergeRequest request)
        {
            this.Namespace = request.Project.PathWithNamespace.Split('/')[0];
            this.ProjectId = request.Project.PathWithNamespace.Split('/')[1];
            this.ProjectName = request.Project.Name;
            this.MergeRequestId = request.ObjectAttributes.Iid.Value;
            this.CommitHash = request.ObjectAttributes.LastCommit.Id;
            this.CommitTitle = request.ObjectAttributes.LastCommit.Title;
            this.Title = request.ObjectAttributes.Title;
            this.CommitMessage = request.ObjectAttributes.LastCommit.Message;
            this.Time = Convert.ToDateTime(request.ObjectAttributes.LastCommit.Timestamp);
            this.AuthorName = request.ObjectAttributes.LastCommit.Author.Name;
            this.AuthorEmail = request.ObjectAttributes.LastCommit.Author.Email;
            this.MergeRequestUrl = request.Project.WebUrl + $"/-/merge_requests/{MergeRequestId}/";
            this.SourceBranch = request.ObjectAttributes.SourceBranch;
        }

        public string Namespace { get; set; }

        public string ProjectId { get; set; }

        public string ProjectName { get; set; }

        public int MergeRequestId { get; set; }

        public string CommitHash { get; set; }

        public string CommitTitle { get; set; }

        public string CommitMessage { get; set; }

        public string Title { get; set; }

        public DateTime Time { get; set; }

        public string AuthorName { get; set; }

        public string AuthorEmail { get; set; }

        public string MergeRequestUrl { get; set; }

        public string SourceBranch { get; set; }
    }
}
