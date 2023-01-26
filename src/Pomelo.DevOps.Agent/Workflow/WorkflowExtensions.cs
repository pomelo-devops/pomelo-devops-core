using Microsoft.Extensions.DependencyInjection;
using Pomelo.Workflow.Storage;

namespace Pomelo.DevOps.Agent.Workflow
{
    public static class WorkflowExtensions
    {
        public static IServiceCollection AddWorkflowServices(this IServiceCollection services)
            => services.AddScoped<AgentWorkflowManager>()
                .AddScoped<IWorkflowStorageProvider, AgentWorkflowStorageProvider>()
                .AddScoped<WorkflowContext>();
    }
}
