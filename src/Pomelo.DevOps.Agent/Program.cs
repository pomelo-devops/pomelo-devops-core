// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pomelo.DevOps.Agent
{
    public class Program
    {
        public static bool IgnoreSslErrors = false;

        public static void Main(string[] args)
        {
            for (var i = 0; i < args.Length; ++i)
            {
                if (args[i].ToLower() == "--ignore-ssl-errors")
                {
                    IgnoreSslErrors = true;
                }
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:5500");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
