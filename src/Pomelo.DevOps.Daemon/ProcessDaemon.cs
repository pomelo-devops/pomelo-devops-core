﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security;
using Newtonsoft.Json;

namespace Pomelo.DevOps.Daemon
{
    public class ProcessDaemon
    {
        private ConcurrentBag<Thread> threads = new ConcurrentBag<Thread>();
        private List<Process> processBag = new List<Process>();

        private int RunUpgrade(Daemon proc, StreamWriter log)
        {
            var scriptPath = Path.Combine(proc.WorkingDirectory, "upgrade.ps1");
            if (!File.Exists(scriptPath))
            {
                log.WriteLine($"[{DateTime.Now}] Upgrade script not found, skipping upgrade.");
                return 0;
            }

            SecureString password = null;
            if (proc.Password != null)
            {
                password = new SecureString();
                foreach (var ch in proc.Password)
                {
                    password.AppendChar(ch);
                }
            }

            if (!Path.IsPathRooted(proc.FileName))
            {
                proc.FileName = Path.Combine(
                    proc.FileName);
            }

            if (proc.WorkingDirectory != null && !Path.IsPathRooted(proc.WorkingDirectory))
            {
                proc.WorkingDirectory = Path.Combine(
                    proc.WorkingDirectory);
            }

            using (password)
            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "/f ./upgrade.ps1",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UserName = proc.Username,
                    UseShellExecute = false,
                    WorkingDirectory = proc.WorkingDirectory
                }
            })
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
#pragma warning disable CA1416
                    process.StartInfo.Password = password;
#pragma warning restore CA1416
                }

                processBag.Add(process);
                process.OutputDataReceived += (sender, e) => { log.WriteLine($"[{DateTime.Now}] {e.Data}"); log.Flush(); };
                process.ErrorDataReceived += (sender, e) => { log.WriteLine($"[{DateTime.Now}] {e.Data}"); log.Flush(); };
                log.WriteLine($"[{DateTime.Now}] Starting upgrade script for {process.StartInfo.FileName}.");
                log.Flush();
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                processBag.Remove(process);
                log.WriteLine($"[{DateTime.Now}] {process.StartInfo.FileName} has exited with code {process.ExitCode}.");
                log.Flush();
                return process.ExitCode;
            }
        }

        private int RunProcess(Daemon proc, StreamWriter log)
        {
            SecureString password = null;
            if (proc.Password != null)
            {
                password = new SecureString();
                foreach (var ch in proc.Password)
                {
                    password.AppendChar(ch);
                }
            }

            if (!Path.IsPathRooted(proc.FileName))
            {
                proc.FileName = Path.Combine(
                    proc.FileName);
            }

            if (proc.WorkingDirectory != null && !Path.IsPathRooted(proc.WorkingDirectory))
            {
                proc.WorkingDirectory = Path.Combine(
                    proc.WorkingDirectory);
            }

            using (password)
            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = proc.FileName,
                    Arguments = proc.Arguments,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UserName = proc.Username,
                    UseShellExecute = false,
                    WorkingDirectory = proc.WorkingDirectory
                }
            })
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
#pragma warning disable CA1416
                    process.StartInfo.Password = password;
#pragma warning restore CA1416
                }

                processBag.Add(process);
                process.OutputDataReceived += (sender, e) => { log.WriteLine($"[{DateTime.Now}] {e.Data}"); log.Flush(); };
                process.ErrorDataReceived += (sender, e) => { log.WriteLine($"[{DateTime.Now}] {e.Data}"); log.Flush(); };
                log.WriteLine($"[{DateTime.Now}] Starting {process.StartInfo.FileName}.");
                log.Flush();
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                processBag.Remove(process);
                log.WriteLine($"[{DateTime.Now}] {process.StartInfo.FileName} has exited with code {process.ExitCode}.");
                log.Flush();
                if (process.ExitCode != 412)
                {
                    Thread.Sleep(3000);
                }
                return process.ExitCode;
            }
        }

        public void OnStart(string[] args)
        {
            var jsonPath = "process.json";

            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException(jsonPath);
            }

            var processes = JsonConvert.DeserializeObject<List<Daemon>>(File.ReadAllText(jsonPath));
            processes.ForEach(proc =>
            {
                Console.WriteLine($"Starting {proc.FileName}...");
                var path = proc.LogsOutputDirectory
                    ?? Path.Combine(
                        "Logs",
                        Path.GetFileNameWithoutExtension(proc.FileName));
                path = Path.Combine(path, DateTime.UtcNow.Ticks + ".log");
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var t = new Thread(() =>
                {
                    using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite))
                    using (var log = new StreamWriter(fs))
                    {
                        while (true)
                        {
                            if (RunProcess(proc, log) == 412)
                            {
                                RunUpgrade(proc, log);
                            }
                        }
                    }
                });
                threads.Add(t);
                t.Start();
            });
        }

        public void OnStop()
        {
            foreach (var p in processBag)
            {
                try { p.Kill(); } catch { }
                try { p.Dispose(); } catch { }
            }

            foreach (var t in threads)
            {
                try
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
#pragma warning disable SYSLIB0006
                        t.Abort();
#pragma warning restore SYSLIB0006
                    }
                }
                catch { }
            }
        }
    }
}
