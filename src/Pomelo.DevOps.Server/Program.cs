// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace Pomelo.DevOps.Server
{
    internal class BindUrl
    { 
        public IPAddress IPAddress { get; set; }

        public int Port { get; set; }

        public bool UseSsl { get; set; }

        public string CertPath { get; set; }

        public string CertPassword { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Program
    {
        internal static bool SelfHost = false;
        internal static string SslCertPath = null;
        internal static string SslCertPass = null;
        internal static List<BindUrl> BindUrls = new List<BindUrl>();

        public static void Main(string[] args)
        {
            for (var i = 0; i < args.Length; ++i)
            {
                if (args[i].ToLower() == "--self-host")
                {
                    SelfHost = true;
                }

                if (args[i].ToLower() == "--bind-url")
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("Missing argument.");
                        Console.Error.WriteLine("--bind-url <url> <pfx_cert_path?> <cert_pass?>");
                        Environment.Exit(1);
                        return;
                    }

                    var url = args[i + 1];
                    var bindUrl = new BindUrl();
                    if (url.StartsWith("http://"))
                    {
                        url = url.Substring("http://".Length);
                        var splited = url.Split(':').ToList();
                        if (splited[0] == "*")
                        {
                            bindUrl.IPAddress = IPAddress.Any;
                        }
                        else if (splited[0] == "*v6")
                        {
                            bindUrl.IPAddress = IPAddress.IPv6Any;
                        }
                        else
                        {
                            bindUrl.IPAddress = IPAddress.Parse(splited[0]);
                        }

                        if (splited.Count != 2)
                        {
                            splited.Add("80");
                        }

                        bindUrl.Port = Convert.ToInt32(splited[1]);
                        bindUrl.UseSsl = false;
                        ++i;
                    }
                    else if (url.StartsWith("https://"))
                    {
                        url = url.Substring("https://".Length);
                        var splited = url.Split(':').ToList();
                        if (splited[0] == "*")
                        {
                            bindUrl.IPAddress = IPAddress.Any;
                        }
                        else if (splited[0] == "*v6")
                        {
                            bindUrl.IPAddress = IPAddress.IPv6Any;
                        }
                        else
                        {
                            bindUrl.IPAddress = IPAddress.Parse(splited[0]);
                        }

                        if (splited.Count != 2)
                        {
                            splited.Add("443");
                        }

                        bindUrl.Port = Convert.ToInt32(splited[1]);
                        bindUrl.UseSsl = true;

                        if (i + 3 >= args.Length)
                        {
                            Console.Error.WriteLine("Missing argument.");
                            Console.Error.WriteLine("--bind-url <url> <pfx_cert_path> <cert_pass>");
                            Environment.Exit(1);
                            return;
                        }

                        if (args[i + 2][0] == '-' || args[i + 3][0] == '-')
                        {
                            Console.Error.WriteLine("Missing argument.");
                            Console.Error.WriteLine("--bind-url <url> <pfx_cert_path> <cert_pass>");
                            Environment.Exit(1);
                            return;
                        }

                        bindUrl.CertPath = args[i + 2];
                        bindUrl.CertPassword = args[i + 3];
                        i += 3;
                    }
                    else
                    {
                        Console.Error.WriteLine("Invalid Url: " + url);
                        Environment.Exit(1);
                        return;
                    }

                    BindUrls.Add(bindUrl);
                }
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var builder = webBuilder.UseStartup<Startup>();
                    if (SelfHost)
                    { 
                        builder.UseKestrel((ctx, opt) =>
                        {
                            foreach (var bind in BindUrls)
                            {
                                opt.Listen(bind.IPAddress, bind.Port, options =>
                                {
                                    if (bind.UseSsl)
                                    {
                                        options.UseHttps(bind.CertPath, bind.CertPassword);
                                    }
                                });
                            }
                        });
                    }
                });
    }
}
