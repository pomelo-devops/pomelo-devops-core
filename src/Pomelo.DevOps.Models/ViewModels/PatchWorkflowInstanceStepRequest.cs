using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Pomelo.Workflow.Models;

namespace Pomelo.DevOps.Models.ViewModels
{
    public class PatchWorkflowInstanceStepRequest
    {
        public StepStatus Status { get; set; }

        public Dictionary<string, JToken> Arguments { get; set; }

        public string Error { get; set; }
    }
}
