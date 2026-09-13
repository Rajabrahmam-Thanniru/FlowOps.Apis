using System;
using System.Collections.Generic;
using FlowOps.Domain.Enums;

namespace FlowOps.Application.Dtos.Workflows;

public class WorkflowStepDto
{
    public Guid Id { get; set; }
    public int StepOrder { get; set; }
    public string Name { get; set; } = default!;
    public StepAssignmentType AssignmentType { get; set; }
    public Guid? AssignedRoleId { get; set; }
    public Guid? AssignedUserId { get; set; }
}

public class WorkflowDefinitionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

public class CreateWorkflowStepDto
{
    public int StepOrder { get; set; }
    public string Name { get; set; } = default!;
    public StepAssignmentType AssignmentType { get; set; }
    public Guid? AssignedRoleId { get; set; }
    public Guid? AssignedUserId { get; set; }
}

public class CreateWorkflowDto
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public List<CreateWorkflowStepDto> Steps { get; set; } = new();
}
