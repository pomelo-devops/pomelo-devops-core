// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pomelo.DevOps.Models.ViewModels;

namespace Pomelo.DevOps.Agent
{
    public class StageStateMachineFactory
    {
        private IServiceProvider _services;

        public StageStateMachineFactory(IServiceProvider services)
        {
            _services = services;
        }

        public async ValueTask RunStageAsync(JobStage stage)
        {
            using (var scope = _services.CreateScope())
            {
                var statemachine = new StageStateMachine(scope, stage);
                await statemachine.TransitAsync();
            }
        }
    }

    public class StageStateMachine
    {
        JobStage _stage;
        Connector _connector;
        List<JobStep> _steps;
        StepManager _gallery;
        VariableContainer _variable;
        VariableContainerFactory _variableFactory;
        LogManager _log;

        public StageStateMachine(
            IServiceScope scope,
            JobStage stage)
        {
            _stage = stage;
            _steps = _stage.Steps.ToList();
            _connector = scope.ServiceProvider.GetRequiredService<Connector>();
            _gallery = scope.ServiceProvider.GetRequiredService<StepManager>();
            _log = scope.ServiceProvider.GetRequiredService<LogManager>();
            _variableFactory = scope.ServiceProvider.GetRequiredService<VariableContainerFactory>();
            _variable = _variableFactory.GetOrCreate(_stage.Id, _stage.Pipeline, _stage.PipelineJobId, _stage.JobNumber);
        }

        public async ValueTask TransitAsync()
        {
            if (_steps.Count == 0)
            {
                return;
            }

            await UpdateCurrentStateAsync();

            try
            {
                while (true)
                {
                    if (!FindNextStep(out var nextStep))
                    {
                        return;
                    }

                    if (_stage.PipelineJobId != default)
                    {
                        await _variable.LoadVariablesAsync(_stage);
                    }
                    _variable.PutBatchVariables(nextStep.Arguments);
                    nextStep.StartedAt = DateTime.UtcNow;
                    nextStep.Status = PipelineJobStatus.Running;

                    if (_stage.PipelineJobId != default)
                    {
                        nextStep.PipelineJobStageId = _stage.Id;
                        await _connector.UpdateStepStatusAsync(_stage, nextStep);
                        _log.LogAsync(_stage, "step-" + nextStep.Id, "Starting step...", LogLevel.Information);
                    }

                    StepExecuteResult result;
                    if (string.IsNullOrWhiteSpace(nextStep.Method))
                    {
                        try
                        {
                            await _gallery.DownloadStepAsync(nextStep.StepId, nextStep.Version, _variable, (message, isError) =>
                            {
                                _log.LogAsync(_stage, "step-" + nextStep.Id, message, isError ? LogLevel.Error : LogLevel.Information);
                            });
                            result = new StepExecuteResult
                            {
                                ExitCode = 0,
                                Result = StepExecuteStatus.Succeeded
                            };
                        }
                        catch (Exception ex)
                        {
                            result = new StepExecuteResult
                            {
                                ExitCode = 1,
                                Result = StepExecuteStatus.Failed,
                                Error = ex.ToString()
                            };
                        }
                    }
                    else
                    {
                        result = await _gallery.InvokeStepAsync(
                            nextStep.StepId,
                            nextStep.Version,
                            nextStep.Method,
                            _variable,
                            (message, isError) =>
                            {
                                _log.LogAsync(_stage, "step-" + nextStep.Id, message, isError ? LogLevel.Error : LogLevel.Information);
                            },
                            nextStep.Id,
                            nextStep.Timeout);
                        ++nextStep.Attempts;
                        if (nextStep.ErrorHandlingMode == ErrorHandlingMode.StderrAsFail && result.Error.Trim().Length > 0)
                        {
                            result.Result = StepExecuteStatus.Failed;
                        }
                    }

                    switch (result.Result)
                    {
                        case StepExecuteStatus.Timeout:
                        case StepExecuteStatus.Failed:
                            if (nextStep.Attempts >= nextStep.Retry + 1 || nextStep.Retry == -1)
                            {
                                nextStep.Status = PipelineJobStatus.Failed;
                                if (nextStep.ErrorHandlingMode == ErrorHandlingMode.IgnoreFail)
                                {
                                    nextStep.Status = PipelineJobStatus.Succeeded;
                                }
                            }
                            else
                            {
                                nextStep.Status = PipelineJobStatus.Waiting;
                                nextStep.StartedAt = null;
                                await _connector.UpdateStepStatusAsync(_stage, nextStep);
                                _log.LogAsync(_stage, "step-" + nextStep.Id, "The step has failed, retrying...", LogLevel.Warning);
                                continue;
                            }
                            break;
                        case StepExecuteStatus.Succeeded:
                            nextStep.Status = PipelineJobStatus.Succeeded;
                            break;
                    }
                    nextStep.FinishedAt = DateTime.UtcNow;
                    if (_stage.PipelineJobId != default)
                    {
                        nextStep.PipelineJobStageId = _stage.Id;
                        await _connector.UpdateStepStatusAsync(_stage, nextStep);
                    }

                    _log.LogAsync(_stage, "step-" + nextStep.Id, "The step is finished", LogLevel.Information);
                    await UpdateCurrentStateAsync();
                }
            }
            finally
            {
                _variableFactory.Remove(_stage.Id);
            }
        }

        private async ValueTask UpdateCurrentStateAsync()
        {
            foreach (var x in _steps)
            {
                if (x.Status == PipelineJobStatus.Running || x.Status == PipelineJobStatus.Waiting)
                {
                    x.Status = PipelineJobStatus.Pending;
                }
            }

            while (true)
            {
                var step = _steps.FirstOrDefault(x => x.Status < PipelineJobStatus.Failed);
                if (step == null)
                {
                    break;
                }
                var index = _steps.ToList().IndexOf(step);
                await _variable.LoadVariablesAsync(_stage);
                if (index == 0)
                {
                    if (step.Condition == RunCondition.CheckVariable && _variable.GetVariable(step.ConditionVariable).ToLower() != "true")
                    {
                        _log.LogAsync(_stage, "stage-" + _stage.Id, $"Checking step {step.Name} condition variable {step.ConditionVariable}, the variable value is {_variable.GetVariable(step.ConditionVariable).ToLower() }", LogLevel.Information);
                        step.Status = PipelineJobStatus.Skipped;
                        step.StartedAt = DateTime.UtcNow;
                        step.FinishedAt = DateTime.UtcNow;
                        step.PipelineJobStageId = _stage.Id;
                        await _connector.UpdateStepStatusAsync(_stage, step);
                        continue;
                    }

                    step.Status = PipelineJobStatus.Waiting;
                    if (_stage.PipelineJobId != default)
                    {
                        step.PipelineJobStageId = _stage.Id;
                        await _connector.UpdateStepStatusAsync(_stage, step);
                    }

                    break;
                }

                if (step.Condition == RunCondition.RequirePreviousTaskFailed && _steps[index - 1].Status == PipelineJobStatus.Failed
                    || step.Condition == RunCondition.RequirePreviousTaskSuccess && _steps[index - 1].Status == PipelineJobStatus.Succeeded
                    || step.Condition == RunCondition.RunAnyway
                    || step.Condition == RunCondition.CheckVariable && _variable.GetVariable(step.ConditionVariable).ToLower() == "true")
                {
                    step.Status = PipelineJobStatus.Waiting;

                    if (_stage.PipelineJobId != default)
                    {
                        step.PipelineJobStageId = _stage.Id;
                        await _connector.UpdateStepStatusAsync(_stage, step);
                    }

                    break;
                }
                else
                {
                    if (step.Condition == RunCondition.CheckVariable)
                    {
                        _log.LogAsync(_stage, "stage-" + _stage.Id, $"Checking step {step.Name} condition variable {step.ConditionVariable}, the variable value is {_variable.GetVariable(step.ConditionVariable).ToLower() }", LogLevel.Information);
                    }
                    step.Status = PipelineJobStatus.Skipped;

                    if (_stage.PipelineJobId != default)
                    {
                        step.PipelineJobStageId = _stage.Id;
                        await _connector.UpdateStepStatusAsync(_stage, step);
                    }

                    continue;
                }
            }

            if (_steps.All(x => x.Status >= PipelineJobStatus.Failed))
            {
                if (_steps.Any(x => x.Status == PipelineJobStatus.Failed))
                {
                    _stage.Status = PipelineJobStatus.Failed;
                }
                else
                {
                    _stage.Status = PipelineJobStatus.Succeeded;
                }

                _stage.FinishedAt = DateTime.UtcNow;
                await _connector.UpdateStageStatusAsync(_stage);
            }
        }

        private bool FindNextStep(out JobStep step)
        {
            step = _steps.FirstOrDefault(x => x.Status == PipelineJobStatus.Waiting);
            if (step == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public static class StageStateMachineFactoryExtensions
    {
        public static IServiceCollection AddStageStateMachineFactory(this IServiceCollection collection)
        {
            return collection.AddSingleton(x => new StageStateMachineFactory(x));
        }
    }
}
