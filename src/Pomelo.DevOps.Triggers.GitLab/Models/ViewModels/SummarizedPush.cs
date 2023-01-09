// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

namespace Pomelo.DevOps.Triggers.GitLab.Models.ViewModels
{
    public class SummarizedPush
    {
        public SummarizedPush(WebHookPush request)
        {
            this.Namespace = request.Project.PathWithNamespace.Split('/')[0];
            this.ProjectId = request.Project.PathWithNamespace.Split('/')[1];
            this.UserName = request.UserName;
            this.UserEmail = request.UserEmail;
            this.ProjectName = request.Project.Name;
            this.ProjectUrl = request.Project.Url;
            this.Commits = request.Commits;
            this.Ref = request.Ref;
        }

        public string Namespace { get; set; }

        public string ProjectId { get; set; }

        public string UserName { get; set; }

        public string UserEmail { get; set; }

        public string ProjectName { get; set; }

        public string ProjectUrl { get; set; }

        public string Ref { get; set; }

        public string Branch
            => Ref.StartsWith("refs/heads/", StringComparison.OrdinalIgnoreCase) 
                ? Ref.Substring("refs/heads/".Length) 
                : null;

        public IEnumerable<WebHookCommit> Commits { get; set; }
    }
}
