// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Reflection;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pomelo.DevOps.Triggers.GitLab.Models;
using Pomelo.DevOps.Triggers.GitLab.Models.ViewModels;

namespace Pomelo.DevOps.Triggers.GitLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebHookController : ControllerBase
    {
        private static HttpClient client = new HttpClient();

        [HttpPost]
        public async ValueTask<IActionResult> Post(
            [FromServices] TriggerContext db,
            CancellationToken cancellationToken = default) 
        {
            using var streamReader = new StreamReader(Request.Body);
            var bodyString = await streamReader.ReadToEndAsync();
            var baseObject = JsonConvert.DeserializeObject<WebHookBase>(bodyString);
            switch (baseObject.ObjectKind)
            {
                case "merge_request":
                    return await PostMergeRequest(
                        db, 
                        JsonConvert.DeserializeObject<WebHookMergeRequest>(bodyString), cancellationToken);
                case "push":
                    return await PostPush(
                        db,
                        JsonConvert.DeserializeObject<WebHookPush>(bodyString), cancellationToken);
                default:
                    return BadRequest("Invalid object_kind");
            }
        }

        private async ValueTask<IActionResult> PostPush(
            TriggerContext db,
            WebHookPush request,
            CancellationToken cancellationToken = default)
        {
            var lastCommit = request.Commits
                .OrderByDescending(x => x.Timestamp)
                .First();

            var namespaceProject = request.Project.PathWithNamespace;
            var commitHash = lastCommit.Id;

            if (await db.TriggerHistories
                .AnyAsync(x => x.Type == TriggerType.Push
                    && x.NamespaceProject == namespaceProject
                    && x.CommitHash == commitHash))
            {
                return Ok("Ignored");
            }

            var summarized = new SummarizedPush(request);
            var dic = new Dictionary<string, string>
            {
                ["GITLAB_NAMESPACE"] = summarized.Namespace,
                ["GITLAB_PROJECT_ID"] = summarized.ProjectId,
                ["GITLAB_LAST_COMMIT_URL"] = lastCommit.Url,
                ["GITLAB_LAST_COMMIT_ID"] = lastCommit.Id,
                ["GITLAB_LAST_COMMIT_TITLE"] = lastCommit.Title,
                ["GITLAB_LAST_COMMIT_MESSAGE"] = lastCommit.Id,
                ["GITLAB_PROJECT_NAME"] = summarized.ProjectName,
                ["GITLAB_AUTHOR_NAME"] = summarized.UserName,
                ["GITLAB_AUTHOR_EMAIL"] = summarized.UserEmail,
                ["GITLAB_TIME"] = lastCommit.Timestamp.ToString(),
                ["GITLAB_PUSH_REF"] = summarized.Ref,
                ["GITLAB_PUSH_BRANCH"] = summarized.Branch,
                ["GITLAB_PUSH_COMMITS_JSON"] = JsonConvert.SerializeObject(summarized.Commits)
            };

            var triggers = await db.Triggers
                .Where(x => x.Enabled)
                .Where(x => x.Type == TriggerType.Push)
                .Where(x => x.GitLabNamespace == summarized.Namespace)
                .Where(x => x.GitLabProject == summarized.ProjectId)
                .Where(x => x.Branch == "*" || x.Branch == summarized.Branch)
                .ToListAsync(cancellationToken);

            var serverConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "server.json");
            var serverConfig = JsonConvert.DeserializeObject<ServerConfig>(await System.IO.File.ReadAllTextAsync(serverConfigPath, cancellationToken));
            var baseUrl = serverConfig.Server;
            var token = serverConfig.Token;

            foreach (var trigger in triggers)
            {
                var temp = JsonConvert.DeserializeObject<Dictionary<string, string>>(trigger.ArgumentsJson);
                if (temp == null)
                {
                    temp = new Dictionary<string, string>();
                }
                PatchDictionary(temp, dic);
                var requestBody = new
                {
                    triggerType = "Trigger",
                    triggerName = trigger.Name,
                    name = PatchMacro(trigger.JobNameTemplate, dic),
                    description = PatchMacro(trigger.JobDescriptionTemplate, dic),
                    arguments = temp
                };
                try
                {
                    using var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    using var message = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/project/${trigger.PomeloDevOpsProject}/pipeline/{trigger.PomeloDevOpsPipeline}/job");
                    message.Headers.Authorization = new AuthenticationHeaderValue("Token", token);
                    message.Content = content;
                    using var response = await client.SendAsync(message, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        return BadRequest(new
                        {
                            StatusCode = response.StatusCode.ToString(),
                            Message = await response.Content.ReadAsStringAsync(cancellationToken)
                        });
                    }

                    db.TriggerHistories.Add(new TriggerHistory
                    {
                        Type = TriggerType.Push,
                        CommitHash = commitHash,
                        NamespaceProject = namespaceProject
                    });

                    await db.SaveChangesAsync(cancellationToken);
                }
                catch
                {
                    // TODO: Retry & Logging
                    throw;
                }
            }

            return Ok("ok");
        }

        private async ValueTask<IActionResult> PostMergeRequest(
            TriggerContext db,
            WebHookMergeRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.ObjectAttributes.Action != "open" && request.ObjectAttributes.Action != "update")
            {
                return Ok("Ignored");
            }

            var namespaceProject = request.Project.PathWithNamespace;
            var commitHash = request.ObjectAttributes.LastCommit.Id;

            if (await db.TriggerHistories
                .AnyAsync(x => x.Type == TriggerType.MergeRequest 
                    && x.NamespaceProject == namespaceProject 
                    && x.CommitHash == commitHash))
            {
                return Ok("Ignored");
            }

            var summarized = new SummarizedMergeRequest(request);
            var dic = new Dictionary<string, string> 
            {
                ["GITLAB_MR_URL"] = summarized.MergeRequestUrl,
                ["GITLAB_NAMESPACE"] = summarized.Namespace,
                ["GITLAB_PROJECT_ID"] = summarized.ProjectId,
                ["GITLAB_PROJECT_NAME"] = summarized.ProjectName,
                ["GITLAB_MR_ID"] = summarized.MergeRequestId.ToString(),
                ["GITLAB_COMMIT_HASH"] = summarized.CommitHash,
                ["GITLAB_COMMIT_MSG"] = summarized.CommitMessage,
                ["GITLAB_MR_TITLE"] = summarized.Title,
                ["GITLAB_AUTHOR_NAME"] = summarized.AuthorName,
                ["GITLAB_AUTHOR_EMAIL"] = summarized.AuthorEmail,
                ["GITLAB_TIME"] = summarized.Time.ToString(),
                ["GITLAB_SOURCE_BRANCH"] = summarized.SourceBranch
            };

            var triggers = await db.Triggers
                .Where(x => x.Enabled)
                .Where(x => x.Type == TriggerType.MergeRequest)
                .Where(x => x.GitLabNamespace == summarized.Namespace)
                .Where(x => x.GitLabProject == summarized.ProjectId)
                .ToListAsync(cancellationToken);

            var serverConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "server.json");
            var serverConfig = JsonConvert.DeserializeObject<ServerConfig>(await System.IO.File.ReadAllTextAsync(serverConfigPath, cancellationToken));
            var baseUrl = serverConfig.Server;
            var token = serverConfig.Token;

            foreach (var trigger in triggers)
            {
                var temp = JsonConvert.DeserializeObject<Dictionary<string, string>>(trigger.ArgumentsJson);
                if (temp == null)
                {
                    temp = new Dictionary<string, string>();
                }
                PatchDictionary(temp, dic);
                var requestBody = new
                {
                    triggerType = "Trigger",
                    triggerName = trigger.Name,
                    name = PatchMacro(trigger.JobNameTemplate, dic),
                    description = PatchMacro(trigger.JobDescriptionTemplate, dic),
                    arguments = temp
                };
                try
                {
                    using var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    using var message = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/project/${trigger.PomeloDevOpsProject}/pipeline/{trigger.PomeloDevOpsPipeline}/job");
                    message.Headers.Authorization = new AuthenticationHeaderValue("Token", token);
                    message.Content = content;
                    using var response = await client.SendAsync(message, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        return BadRequest(new
                        {
                            StatusCode = response.StatusCode.ToString(),
                            Message = await response.Content.ReadAsStringAsync(cancellationToken)
                        });
                    }

                    db.TriggerHistories.Add(new TriggerHistory 
                    {
                        Type = TriggerType.MergeRequest,
                        CommitHash = commitHash,
                        NamespaceProject = namespaceProject
                    });

                    await db.SaveChangesAsync(cancellationToken);
                }
                catch 
                {
                    // TODO: Retry & Logging
                    throw;
                }
            }

            return Ok("ok");
        }

        private void PatchDictionary(Dictionary<string, string> @base, IDictionary<string, string> additional)
        { 
            if (@base == null)
            {
                return;
            }

            foreach(var item in additional)
            {
                @base[item.Key] = item.Value;
            }
        }

        private string PatchMacro(string template, IDictionary<string, string> dic)
        {
            if (dic == null)
            {
                return template;
            }

            foreach (var item in dic.OrderByDescending(x => x.Key))
            {
                template = template.Replace($"$({item.Key})", item.Value);
            }
            return template;
        }
    }
}
