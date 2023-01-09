// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Web;
using Newtonsoft.Json;
using Pomelo.DevOps.Agent.CLI;

var agentHost = Environment.GetEnvironmentVariable("AGENT_API_HOST")
    ?? "http://localhost:5500";
var projectId = Environment.GetEnvironmentVariable("PIPELINE_PROJECT");
var pipelineId = Environment.GetEnvironmentVariable("PIPELINE_ID");
var jobNumber = Environment.GetEnvironmentVariable("JOB_NUMBER");
var jobEndpoint = $"{agentHost}/api/project/{projectId}/pipeline/{pipelineId}/job/{jobNumber}";

var projectIdArgument = args.FirstOrDefault(x => x.StartsWith("--pipeline-id:", StringComparison.OrdinalIgnoreCase));
if (projectIdArgument != null)
{
    projectId = projectIdArgument.Substring("--pipeline-id:".Length);
}

using var client = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) };

var command = args.FirstOrDefault(x => !x.StartsWith("-"))?.ToLower();
if (command == null)
{
    Console.WriteLine("Pomelo DevOps CLI v0.9.0");
    Console.WriteLine("Project: " + projectId);
    Console.WriteLine("Pipeline: " + pipelineId);
    Console.WriteLine("Job: #" + jobNumber);
    // TODO: Show help
    return;
}

var index = Array.IndexOf(args, command);
if (command == "set-variable")
{
    var variableKey = args[index + 1];
    var variableValue = args[index + 2];
    var body = new { value = variableValue };
    await RestHelper.PostAsync($"{jobEndpoint}/variable/{HttpUtility.UrlEncode(variableKey)}", body);
    return;
}
else if (command == "rename-job")
{ 
    var newJobName = args[index + 1];
    var body = new { name = newJobName };
    await RestHelper.PostAsync($"{jobEndpoint}/misc/rename-job", body);
    return;
}
else if (command == "add-label")
{
    var newJobName = args[index + 1];
    var body = new { label = newJobName };
    await RestHelper.PostAsync($"{jobEndpoint}/misc/add-label", body);
    return;
}