// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Agent.Models;
using Pomelo.DevOps.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Pomelo.DevOps.Agent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemoteController : ControllerBase
    {
        [HttpPost("step")]
        public async ValueTask<ApiResult<AdhocStep>> Post(
            [FromBody] AdhocStep step,
            [FromServices] IServiceProvider provider,
            [FromServices] AdhocStepContainer container,
            CancellationToken cancellationToken = default)
        {
            step.Id = Guid.NewGuid();
            container.Put(step.Id, step);
            Task.Factory.StartNew(async () => {
                using (var scope = provider.CreateScope())
                {
                    var _gallery = scope.ServiceProvider.GetRequiredService<StepManager>();
                    var _variableFactory = scope.ServiceProvider.GetRequiredService<VariableContainerFactory>();
                    var variable = _variableFactory.GetOrCreate(step.Id, null, default, default);
                    try
                    {
                        if (step.Arguments != null)
                        {
                            variable.PutBatchVariables(step.Arguments);
                        }
                        _gallery.AddFeed(step.Feed);
                        var func = new Action<string, bool>((message, isError) =>
                        {
                            if (isError)
                            {
                                step.NewError += message;
                                step.ErrorFull += message;
                            }
                            else
                            {
                                step.NewOutput += message;
                                step.OutputFull += message;
                            }
                        });
                        if (string.IsNullOrWhiteSpace(step.Method))
                        {
                            await _gallery.DownloadStepAsync(step.Step, step.Version, variable, func);
                            step.FinishedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            var result = await _gallery.InvokeStepAsync(
                                step.Step,
                                step.Version,
                                step.Method, 
                                variable, 
                                func, 
                                default,
                                step.Timeout);
                            step.FinishedAt = DateTime.UtcNow;
                            step.OutputFull = result.Output;
                            step.ErrorFull = result.Error;
                            step.ExitCode = result.ExitCode;
                        }
                    }
                    catch(Exception ex)
                    {
                        step.FinishedAt = DateTime.UtcNow;
                        step.NewError = ex.ToString();
                        step.ErrorFull = step.NewError;
                        step.ExitCode = 1;
                    }
                    finally 
                    {
                        _variableFactory.Remove(step.Id);
                    }
                }
            });
            return ApiResult(step);
        }

        [HttpGet("step/{id}")]
        public ApiResult<AdhocStep> Get(
            [FromServices] AdhocStepContainer container,
            Guid id)
        {
            var step = container.Get(id);
            var copy = JsonConvert.DeserializeObject<AdhocStep>(JsonConvert.SerializeObject(step));
            step.NewOutput = "";
            step.NewError = "";
            if (step.FinishedAt.HasValue)
            {
                container.Remove(id);
            }
            else
            {
                copy.OutputFull = null;
                copy.ErrorFull = null;
            }
            return ApiResult(copy);
        }
    }
}
