using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using Newtonsoft.Json;

namespace Pomelo.DevOps.Models
{
    public enum PipelineArgumentType
    { 
        Text,
        Option,
        ToggleButton,
        Number
    }

    public class PipelineArgument
    {
        [MaxLength(64)]
        [ForeignKey(nameof(Pipeline))]
        [YamlIgnore]
        public string PipelineId { get; set; }

        [YamlIgnore]
        [JsonIgnore]
        public virtual Pipeline Pipeline { get; set; }

        [MaxLength(64)]
        [YamlMember(Alias = "argument")]
        public string Argument { get; set; }

        [MaxLength(64)]
        [YamlMember(Alias = "description", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string Description { get; set; }

        [YamlMember(Alias = "type")]
        public PipelineArgumentType Type { get; set; }

        [YamlMember(Alias = "priority", DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public int Priority { get; set; }

        [YamlMember(Alias = "validate_regex", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string ValidateRegex { get; set; }

        [YamlMember(Alias = "options", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string Options { get; set; }

        [YamlMember(Alias = "number_step", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public float? NumberStep{ get; set; }

        [YamlMember(Alias = "number_min", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public float? NumberMin { get; set; }

        [YamlMember(Alias = "number_max", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public float? NumberMax { get; set; }

        [YamlMember(Alias = "default", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string Default { get; set; }
    }
}
