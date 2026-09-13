using Microsoft.Extensions.DependencyInjection;

namespace FlowOps.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<FlowOps.Application.Common.Interfaces.IWorkflowService, FlowOps.Application.Services.WorkflowService>();
        services.AddScoped<FlowOps.Application.Common.Interfaces.IRequestEngine, FlowOps.Application.Services.RequestEngine>();
        return services;
    }
}
