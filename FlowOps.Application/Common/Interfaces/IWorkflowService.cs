using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowOps.Application.Dtos.Workflows;

namespace FlowOps.Application.Common.Interfaces;

public interface IWorkflowService
{
    Task<List<WorkflowDefinitionDto>> GetWorkflowsAsync(CancellationToken cancellationToken = default);
    Task<WorkflowDefinitionDto> GetWorkflowByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkflowDefinitionDto> CreateWorkflowAsync(CreateWorkflowDto request, CancellationToken cancellationToken = default);
    Task ToggleWorkflowActiveStatusAsync(Guid id, CancellationToken cancellationToken = default);
}
