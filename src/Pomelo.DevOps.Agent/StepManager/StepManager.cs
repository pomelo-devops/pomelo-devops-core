// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Pomelo.DevOps.Shared;
using Microsoft.Extensions.DependencyInjection;
using YamlDotNet.Serialization;
using System.Reflection;

namespace Pomelo.DevOps.Agent
{
    public struct StepExecuteResult
    {
        public string Output { get; set; }

        public string Error { get; set; }

        public int ExitCode { get; set; }

        public StepExecuteStatus Result { get; set; }
    }

    public enum StepExecuteStatus
    {
        Succeeded,
        Failed,
        Timeout
    }

    public class StepManager : IDisposable
    {
        private HttpClient _client;
        private readonly string _stepFolder;
        private readonly List<string> _feeds = new List<string>();
        private readonly IDeserializer _yamlDeserializer;
        private readonly ConfigManager _config;
        private readonly MetricsLogger _metrics;

        public StepManager(ConfigManager config, MetricsLogger metrics)
        {
            _client = new HttpClient();
            _yamlDeserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            _config = config;
            _metrics = metrics;
            _stepFolder = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                "Cache", 
                "Steps");
            if (!Directory.Exists(_stepFolder))
            {
                Directory.CreateDirectory(_stepFolder);
            }

            if (_config.Config.GalleryFeeds != null)
            {
                _feeds.AddRange(_config.Config.GalleryFeeds);
            }
        }

        public void ClearFeed()
        {
            lock(this)
            {
                _feeds.Clear();
            }
        }

        public void AddFeed(string feed)
        {
            lock (this)
            {
                if (_feeds.Contains(feed))
                {
                    return;
                }

                _feeds.Add(feed);
            }
        }

        public async ValueTask<StepExecuteResult> InvokeStepAsync(string step, string version, string method, VariableContainer variable = null, Action<string, bool>logFunc = null, Guid stepId = default, int timeout = -1)
        {
            using (var process = new Process())
            {
                var _step = await DownloadStepAsync(step, version, variable, logFunc);
                var stepFolder = Path.Combine(_stepFolder, step, version);
                process.StartInfo = new ProcessStartInfo();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = stepFolder;
                if (_step.Methods == null)
                {
                    Console.Error.WriteLine("No available method found");
                    return new StepExecuteResult
                    {
                        Error = "No available method found",
                        ExitCode = -1,
                        Result = StepExecuteStatus.Failed
                    };
                }
                var _method = _step.Methods.FirstOrDefault(x => x.Name == method);
                if (_method == null)
                {
                    throw new StepPackageMethodNotFoundException(_step.Id, _step.Version, method);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (_method.Entry.Windows == null)
                    {
                        throw new NotSupportedException("The step does not support Windows platform");
                    }

                    var splited = _method.Entry.Windows.Split(' ');
                    process.StartInfo.FileName = splited[0];
                    process.StartInfo.Arguments = string.Join(' ', splited.Skip(1).ToArray());
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (_method.Entry.Linux == null)
                    {
                        throw new NotSupportedException("The step does not support Linux platform");
                    }

                    var splited = _method.Entry.Linux.Split(' ');
                    process.StartInfo.FileName = splited[0];
                    process.StartInfo.Arguments = string.Join(' ', splited.Skip(1).ToArray());
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (_method.Entry.Mac == null)
                    {
                        throw new NotSupportedException("The step does not support Mac platform");
                    }

                    var splited = _method.Entry.Linux.Split(' ');
                    process.StartInfo.FileName = splited[0];
                    process.StartInfo.Arguments = string.Join(' ', splited.Skip(1).ToArray());
                }
                else
                {
                    throw new NotSupportedException("The step does not support this platform");
                }

                if (variable != null)
                {
                    foreach (var argument in variable.Keys)
                    {
                        var value = variable.GetVariable(argument);
                        if (value.Length < 256)
                        {
                            process.StartInfo.EnvironmentVariables.Add(argument, value);
                        }
                    }

                    if (variable.Keys.Contains("WorkingDirectory"))
                    {
                        process.StartInfo.WorkingDirectory = variable.GetVariable("WorkingDirectory");
                    }
                }

                var output = "";
                var error = "";

                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e)=>
                {
                    if (string.IsNullOrWhiteSpace(e.Data))
                    {
                        return;
                    }
                    error += e.Data;
                    Console.Error.WriteLine(e.Data);
                    if (logFunc != null)
                    {
                        logFunc(e.Data, true);
                    }
                };
                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.Data))
                    {
                        return;
                    }
                    output += e.Data;
                    Console.WriteLine(e.Data);
                    if (logFunc != null)
                    {
                        logFunc(e.Data, false);
                    }
                };

                process.StartInfo.Arguments = variable.EscapeString(process.StartInfo.Arguments);
                var agentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                process.StartInfo.EnvironmentVariables["PATH"] = process.StartInfo.EnvironmentVariables["PATH"] + ";" + stepFolder + ";" + agentFolder + ";";
                if (!File.Exists(process.StartInfo.FileName))
                {
                    var physicalPath = Path.Combine(stepFolder, process.StartInfo.FileName);
                    if (File.Exists(physicalPath)) 
                    {
                        process.StartInfo.FileName = physicalPath;
                    }
                }
                try
                {
                    _metrics.StartMonitor();
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    if (timeout == -1)
                    {
                        process.WaitForExit();
                    }
                    else
                    {
                        if (!process.WaitForExit(timeout * 1000))
                        {
                            process.Kill();
                            return new StepExecuteResult
                            {
                                Error = "Timeout",
                                Result = StepExecuteStatus.Timeout,
                                Output = "",
                                ExitCode = process.ExitCode
                            };
                        }
                    }

                    if (process.ExitCode == 0)
                    {
                        return new StepExecuteResult
                        {
                            Error = error,
                            Output = output,
                            Result = StepExecuteStatus.Succeeded,
                            ExitCode = process.ExitCode
                        };
                    }
                    else
                    {
                        return new StepExecuteResult
                        {
                            Error = error,
                            Output = output,
                            Result = StepExecuteStatus.Failed,
                            ExitCode = process.ExitCode
                        };
                    }
                }
                finally
                {
                    _metrics.StopMonitor();
                }
            }
        }

        public async ValueTask InstallStepAsync(string step, string version, VariableContainer variable, int timeout, Action<string, bool>logFunc)
        {
            using (var process = new Process())
            {
                var _step = await DownloadStepAsync(step, version, variable, logFunc);
                var stepFolder = Path.Combine(_stepFolder, step, version);
                process.StartInfo = new ProcessStartInfo();
                process.StartInfo.WorkingDirectory = stepFolder;
                if (_step.Install == null)
                {
                    return;
                }
                if (variable != null)
                {
                    foreach (var argument in variable.Keys)
                    {
                        var value = variable.GetVariable(argument);
                        if (value.Length < 256)
                        {
                            process.StartInfo.EnvironmentVariables.Add(argument, value);
                        }
                    }

                    if (variable.Keys.Contains("WorkingDirectory"))
                    {
                        process.StartInfo.WorkingDirectory = variable.GetVariable("WorkingDirectory");
                    }
                }
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (_step.Install.Windows == null)
                    {
                        throw new NotSupportedException("The step does not support Windows platform");
                    }

                    var splited = _step.Install.Windows.Split(' ');
                    process.StartInfo.FileName = splited[0];
                    process.StartInfo.Arguments = string.Join(' ', splited.Skip(1).ToArray());
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (_step.Install.Linux == null)
                    {
                        throw new NotSupportedException("The step does not support Linux platform");
                    }

                    var splited = _step.Install.Linux.Split(' ');
                    process.StartInfo.FileName = splited[0];
                    process.StartInfo.Arguments = string.Join(' ', splited.Skip(1).ToArray());
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (_step.Install.Mac == null)
                    {
                        throw new NotSupportedException("The step does not support Mac platform");
                    }

                    var splited = _step.Install.Linux.Split(' ');
                    process.StartInfo.FileName = splited[0];
                    process.StartInfo.Arguments = string.Join(' ', splited.Skip(1).ToArray());
                }
                else
                {
                    throw new NotSupportedException("The step does not support this platform");
                }

                var output = "";
                var error = "";

                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.Data))
                    {
                        return;
                    }
                    error += e.Data;
                    Console.Error.WriteLine(e.Data);
                    if (logFunc != null)
                    {
                        logFunc(e.Data, true);
                    }
                };
                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.Data))
                    {
                        return;
                    }
                    output += e.Data;
                    Console.WriteLine(e.Data);
                    if (logFunc != null)
                    {
                        logFunc(e.Data, false);
                    }
                };

                process.StartInfo.Arguments = variable.EscapeString(process.StartInfo.Arguments);
                try
                {
                    _metrics.StartMonitor();
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    if (timeout == -1)
                    {
                        process.WaitForExit();
                    }
                    else
                    {
                        if (!process.WaitForExit(timeout * 1000))
                        {
                            process.Kill();
                            return;
                        }
                    }
                    if (process.ExitCode == 0)
                    {
                        UpdateEnvironmentVariablePath();
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                finally
                {
                    _metrics.StopMonitor();
                }
            }
        }

        private void UpdateEnvironmentVariablePath()
        {
            var paths = string.Join(';', new[] 
            {
                Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine),
                Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User),
                Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process),
            });
            Environment.SetEnvironmentVariable(
                "PATH", 
                string.Join(';', paths.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()), 
                EnvironmentVariableTarget.Process);
        }


        public async ValueTask<Step> ReadStepInfoAsync(string step, string version)
        {
            var path = Path.Combine(_stepFolder, step, version, "step.yml");
            if (File.Exists(path))
            {
                return _yamlDeserializer.Deserialize<Step>(await File.ReadAllTextAsync(path));
            }
            else
            {
                return null;
            }
        }

        public async ValueTask<Step> DownloadStepAsync(string step, string version, VariableContainer variable, Action<string, bool>logFunc)
        {
            var _step = await ReadStepInfoAsync(step, version);
            if (_step != null)
            {
                return _step;
            }

            foreach (var feed in _feeds)
            {
                try
                {
                    var info = await DownloadStepAsync(feed, step, version);
                    if (info != null)
                    {
                        if (info.Dependencies != null)
                        {
                            foreach (var dependency in info.Dependencies)
                            {
                                await DownloadStepAsync(dependency.Key, dependency.Value, variable, logFunc);
                            }
                        }
                        if (info.Install != null && info.InstallTimeout.HasValue)
                        {
                            await InstallStepAsync(step, version, variable, info.InstallTimeout.Value, logFunc);
                        }
                        return info;
                    }
                }
                catch { }
            }

            throw new StepPackageNotFoundException(_feeds, step, version);
        }

        private async ValueTask<Step> DownloadStepAsync(string feed, string step, string version)
        {
            using (var response = await _client.GetAsync($"{feed.TrimEnd('/')}/api/gallery/{step}/version/{version}/package.pdo"))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var pdo = new PomeloDevOpsPackage(stream))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                var info = await pdo.DeserializeYamlAsync();
                var stepFolder = Path.Combine(_stepFolder, info.Id);
                if (!Directory.Exists(stepFolder))
                {
                    Directory.CreateDirectory(stepFolder);
                }
                var versionFolder = Path.Combine(stepFolder, info.Version);
                if (!Directory.Exists(versionFolder))
                {
                    Directory.CreateDirectory(versionFolder);
                }
                pdo.ExtractToDirectory(versionFolder, true);
                Chmod(versionFolder);
                return info;
            }
        }

        private void Chmod(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
            foreach(var x in files)
            {
                using (var process = Process.Start("chmod", "u+x " + x))
                {
                    process.WaitForExit();
                }
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }

    public static class GalleryManagerExtensions
    {
        public static IServiceCollection AddGalleryManager(this IServiceCollection collection)
        {
            return collection.AddSingleton<StepManager>();
        }
    }
}
