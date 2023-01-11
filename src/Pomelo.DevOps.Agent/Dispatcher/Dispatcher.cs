﻿// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Agent
{
    public class Dispatcher
    {
        private readonly ConfigManager _config;
        private readonly Connector _connector;
        private readonly StageStateMachineFactory _factory;
        private readonly StageContainer _stageContainer;
        private readonly LogManager _log;
        private readonly IDeserializer _yamlDeserializer;
        private string jobId;
        public string JobId => jobId;

        public Dispatcher(
            ConfigManager config,
            Connector connector,
            StageStateMachineFactory factory,
            StageContainer stageContainer,
            LogManager log)
        {
            _config = config;
            _connector = connector;
            _factory = factory;
            _log = log;
            _stageContainer = stageContainer;
            _yamlDeserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        }

        public Task PollAsync(IsolationLevel mode)
        {
            return Task.Run(async ()=> 
            {
                while (true)
                {
                    try
                    {
                        if (_config.Config.AgentId == null)
                        {
                            await Task.Delay(5000);
                            _config.Reload();
                            continue;
                        }

                        if (mode == IsolationLevel.Sequential)
                        {
                            if (await HandleSingleAsync(mode))
                            {
                                continue;
                            }
                            else
                            {
                                await Task.Delay(5000);
                            }
                        }
                        else
                        {
                            await HandleSingleAsync(mode);
                            await Task.Delay(5000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex.ToString());
                        jobId = null;
                        await Task.Delay(10000);
                    }
                }
            });
            
        }

        private async ValueTask<bool> HandleSingleAsync(IsolationLevel mode)
        {
            JobStage stage = null;
            try
            {
                var yml = await _connector.GetWaitingJobAsync(_stageContainer.GetStageIds(), mode);
                stage = _yamlDeserializer.Deserialize<JobStage>(yml);
                jobId = stage.PipelineJobId.ToString();
                Console.WriteLine($"Start to run {jobId}");
                _stageContainer.Add(stage);
                if (mode == IsolationLevel.Parallel)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _factory.RunStageAsync(stage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            throw;
                        }
                        finally
                        {
                            try
                            {
                                _stageContainer.Remove(stage.Id);
                            }
                            catch { }
                        }
                    });
                }
                else
                {
                    await _factory.RunStageAsync(stage);
                }
                return true;
            }
            catch (ConnectorException)
            {
                return false;
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                if (stage != null)
                {
                    try
                    {
                        stage.Status = PipelineJobStatus.Failed;
                        stage.FinishedAt = DateTime.UtcNow;
                        await _connector.UpdateStageStatusAsync(stage);
                        await _log.LogAsync(stage, "stage-" + stage.Id, ex.ToString(), LogLevel.Error);
                        Console.Error.WriteLine(ex.ToString());
                    }
                    catch { }
                }
                throw;
            }
            finally
            {
                jobId = null;
                if (stage != null)
                {
                    try
                    {
                        if (mode == IsolationLevel.Sequential)
                        {
                            _stageContainer.Remove(stage.Id);
                        }
                    }
                    catch { }
                }
            }
        }
    }

    public static class DispatcherExtensions
    {
        public static IServiceCollection AddDispatcher(this IServiceCollection collection)
        {
            return collection.AddSingleton<Dispatcher>();
        }
    }
}