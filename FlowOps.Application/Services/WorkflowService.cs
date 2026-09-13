using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowOps.Application.Common.Interfaces;
using FlowOps.Application.Dtos.Workflows;
using FlowOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowOps.Application.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IFlowOpsDbContext _context;

    public WorkflowService(IFlowOpsDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkflowDefinitionDto>> GetWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        var workflows = await _context.WorkflowDefinitions
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        return workflows.Select(MapToDto).ToList();
    }

    public async Task<WorkflowDefinitionDto> GetWorkflowByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workflow = await _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        if (workflow == null) throw new Exception("Workflow not found.");

        workflow.Steps = workflow.Steps.OrderBy(s => s.StepOrder).ToList();
        return MapToDto(workflow);
    }

    public async Task<WorkflowDefinitionDto> CreateWorkflowAsync(CreateWorkflowDto request, CancellationToken cancellationToken = default)
    {
        var workflow = new WorkflowDefinition
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            Version = 1,
            Steps = request.Steps.Select(s => new WorkflowStep
            {
                Name = s.Name,
                StepOrder = s.StepOrder,
                AssignmentType = s.AssignmentType,
                AssignedRoleId = s.AssignedRoleId,
                AssignedUserId = s.AssignedUserId
            }).ToList()
        };

        _context.WorkflowDefinitions.Add(workflow);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(workflow);
    }

    public async Task ToggleWorkflowActiveStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(new object[] { id }, cancellationToken);
        if (workflow == null) throw new Exception("Workflow not found.");

        workflow.IsActive = !workflow.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static WorkflowDefinitionDto MapToDto(WorkflowDefinition entity)
    {
        return new WorkflowDefinitionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Version = entity.Version,
            IsActive = entity.IsActive,
            Steps = entity.Steps.Select(s => new WorkflowStepDto
            {
                Id = s.Id,
                StepOrder = s.StepOrder,
                Name = s.Name,
                AssignmentType = s.AssignmentType,
                AssignedRoleId = s.AssignedRoleId,
                AssignedUserId = s.AssignedUserId
            }).ToList()
        };
    }
}
