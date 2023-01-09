// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace Pomelo.DevOps.Agent
{
    public class ConfigManager
    {
        private string _configFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json");

        public ConfigManager()
        {
            Reload();
        }

        public void Reload()
        {
            lock(this)
            {
                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(_configFilePath), new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Populate });
            }
        }

        public void Save()
        {
            lock(this)
            {
                File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(Config));
            }
        }

        public Config Config { get; private set; }
    }

    public static class ConfigManagerExtensions
    {
        public static IServiceCollection AddConfigManager(this IServiceCollection collection)
        {
            return collection.AddSingleton<ConfigManager>();
        }
    }
}
