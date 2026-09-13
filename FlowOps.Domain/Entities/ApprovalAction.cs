using System;
using FlowOps.Domain.Common;
using FlowOps.Domain.Enums;

namespace FlowOps.Domain.Entities;

public class ApprovalAction : BaseEntity
{
    public Guid WorkflowRequestId { get; set; }
    public WorkflowRequest WorkflowRequest { get; set; } = default!;

    public Guid StepId { get; set; }
    public WorkflowStep Step { get; set; } = default!;

    public Guid PerformedById { get; set; }
    public User PerformedBy { get; set; } = default!;

    public ActionType Action { get; set; }
    public string? Reason { get; set; }
}
