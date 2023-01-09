// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pomelo.DevOps.Triggers.Scheduled.Models;
using Pomelo.DevOps.Triggers.Scheduled.Models.ViewModels;

namespace Pomelo.DevOps.Triggers.Scheduled
{
    public class Scheduler : IDisposable
    {
        private IServiceProvider services;
        private HttpClient client = new HttpClient();
        private Thread thread;

        public Scheduler(IServiceProvider services)
        {
            this.services = services;
        }

        public void Dispose()
        {
            client?.Dispose();
        }

        public void Start()
        {
            thread = new Thread(async () =>
            {
                while (true)
                {
                    Handle(DateTime.UtcNow);
                    await Task.Delay(60 * 1000);
                }
            });
            thread.Start();
        }

        private async Task Handle(DateTime time)
        { 
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetService<TriggerContext>();
            var triggers = await db.Triggers
                .AsNoTracking()
                .Where(x => x.Enabled)
                .ToListAsync();

            foreach (var trigger in triggers)
            {
                var dayOfWeek = trigger.DayOfWeek
                    .Split(',')
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => Enum.Parse<DayOfWeek>(x.Trim()))
                    .ToArray();

                if (trigger.Hour != time.Hour || trigger.Minute != time.Minute || !dayOfWeek.Contains(time.DayOfWeek))
                {
                    continue;
                }

                var dic = new Dictionary<string, string>
                {
                    ["SCHEDULED_JOB_NAME"] = trigger.Name,
                    ["SCHEDULED_JOB_CURRENT_TIME"] = DateTime.UtcNow.ToString()
                };

                var serverConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "server.json");
                var serverConfig = JsonConvert.DeserializeObject<ServerConfig>(await File.ReadAllTextAsync(serverConfigPath));
                var baseUrl = serverConfig.Server;
                var token = serverConfig.Token;

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
                    using var response = await client.SendAsync(message);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new BadHttpRequestException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    }
                }
                catch
                {
                    // TODO: Retry & Logging
                }
            }
        }

        private void PatchDictionary(Dictionary<string, string> @base, IDictionary<string, string> additional)
        {
            if (@base == null)
            {
                return;
            }

            foreach (var item in additional)
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
