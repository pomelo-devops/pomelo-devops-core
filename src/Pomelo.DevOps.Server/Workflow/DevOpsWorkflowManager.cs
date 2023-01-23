using System;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.Workflow;
using Pomelo.Workflow.Storage;

namespace Pomelo.DevOps.Server.Workflow
{
    public class DevOpsWorkflowManager : WorkflowManager
    {
        public DevOpsWorkflowManager(
            IServiceProvider services, 
            IWorkflowStorageProvider storage) 
            : base(services, storage)
        {
        }

        public override string DefaultWorkflowDiagramTemplate => @"{
    ""guid"": ""{DIAGRAM_GUID}"",
    ""shapes"": [
        {
            ""guid"": ""{START_NODE_GUID}"",
            ""points"": [
                {
                    ""x"": 26,
                    ""y"": 22
                },
                {
                    ""x"": 146,
                    ""y"": 22
                },
                {
                    ""x"": 146,
                    ""y"": 62
                },
                {
                    ""x"": 26,
                    ""y"": 62
                }
            ],
            ""anchors"": [
                {
                    ""xPercentage"": 0.5,
                    ""yPercentage"": 0
                },
                {
                    ""xPercentage"": 1,
                    ""yPercentage"": 0.5
                },
                {
                    ""xPercentage"": 0.5,
                    ""yPercentage"": 1
                },
                {
                    ""xPercentage"": 0,
                    ""yPercentage"": 0.5
                }
            ],
            ""node"": ""start"",
            ""type"": ""Rectangle"",
            ""width"": 120,
            ""height"": 40
        },
        {
            ""guid"": ""{FINISH_NODE_GUID}"",
            ""points"": [
                {
                    ""x"": 26,
                    ""y"": 315
                },
                {
                    ""x"": 146,
                    ""y"": 315
                },
                {
                    ""x"": 146,
                    ""y"": 355
                },
                {
                    ""x"": 26,
                    ""y"": 355
                }
            ],
            ""anchors"": [
                {
                    ""xPercentage"": 0.5,
                    ""yPercentage"": 0
                },
                {
                    ""xPercentage"": 1,
                    ""yPercentage"": 0.5
                },
                {
                    ""xPercentage"": 0.5,
                    ""yPercentage"": 1
                },
                {
                    ""xPercentage"": 0,
                    ""yPercentage"": 0.5
                }
            ],
            ""node"": ""finish"",
            ""type"": ""Rectangle"",
            ""width"": 120,
            ""height"": 40
        }
    ],
    ""connectPolylines"": [
        {
            ""guid"": ""{POLYLINE_GUID}"",
            ""departureShapeGuid"": ""{START_NODE_GUID}"",
            ""destinationShapeGuid"": ""{FINISH_NODE_GUID}"",
            ""departureAnchorIndex"": 2,
            ""destinationAnchorIndex"": 0,
            ""color"": ""#56a333"",
            ""path"": [
                {
                    ""x"": 86,
                    ""y"": 62
                },
                {
                    ""x"": 86,
                    ""y"": 72
                },
                {
                    ""x"": 86,
                    ""y"": 315
                }
            ],
            ""type"": ""default"",
            ""arguments"": null,
            ""dashed"": false
        }
    ]
}";
    }

    public static class DevOpsWorkflowManagerExtensions
    {
        public static IServiceCollection AddDevOpsWorkflowManager(this IServiceCollection services)
            => services.AddScoped<DevOpsWorkflowManager>();
    }
}
