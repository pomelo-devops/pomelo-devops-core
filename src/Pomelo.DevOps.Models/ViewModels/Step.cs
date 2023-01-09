using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Pomelo.DevOps.Models.ViewModels
{
    public class Step
    {
        [YamlMember(Alias = "id")]
        public string Id { get; set; }

        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "version")]
        public string Version { get; set; }

        [YamlMember(Alias = "description", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string Description { get; set; }

        [YamlMember(Alias = "dependencies", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public Dictionary<string, string> Dependencies { get; set; }

        [YamlMember(Alias = "install", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public StepExecuteCommand Install { get; set; }

        [YamlMember(Alias = "install_timeout", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public int? InstallTimeout { get; set; }

        [YamlMember(Alias = "methods", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public IEnumerable<StepExecuteMethod> Methods { get; set; }

        [YamlMember(Alias = "website", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string Website { get; set; }
    }

    public class StepExecuteMethod
    {
        [YamlMember(Alias = "name", Order = 1)]
        public string Name { get; set; }

        [YamlMember(Alias = "entry", Order = 2)]
        public StepExecuteCommand Entry { get; set; }
    }

    public class StepExecuteCommand
    {
        [YamlMember(Alias = "windows", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string Windows { get; set; }

        [YamlMember(Alias = "linux", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string Linux { get; set; }

        [YamlMember(Alias = "mac", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string Mac { get; set; }
    }
}
