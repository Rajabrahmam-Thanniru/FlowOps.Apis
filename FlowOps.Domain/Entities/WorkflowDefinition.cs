using System;
using System.Collections.Generic;
using FlowOps.Domain.Common;

namespace FlowOps.Domain.Entities;

public class WorkflowDefinition : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public ICollection<WorkflowRequest> Requests { get; set; } = new List<WorkflowRequest>();
}
