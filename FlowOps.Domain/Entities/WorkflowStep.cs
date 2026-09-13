using System;
using FlowOps.Domain.Common;
using FlowOps.Domain.Enums;

namespace FlowOps.Domain.Entities;

public class WorkflowStep : BaseEntity
{
    public int StepOrder { get; set; }
    public string Name { get; set; } = default!;
    public StepAssignmentType AssignmentType { get; set; }
    
    public Guid? AssignedRoleId { get; set; }
    public Role? AssignedRole { get; set; }

    public Guid? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public Guid WorkflowDefinitionId { get; set; }
    public WorkflowDefinition WorkflowDefinition { get; set; } = default!;
}
