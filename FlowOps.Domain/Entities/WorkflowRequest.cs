using System;
using System.Collections.Generic;
using FlowOps.Domain.Common;
using FlowOps.Domain.Enums;

namespace FlowOps.Domain.Entities;

public class WorkflowRequest : BaseEntity
{
    public string RequestNumber { get; set; } = default!; 
    public string Title { get; set; } = default!;
    public string FormPayloadJson { get; set; } = "{}";
    public RequestStatus Status { get; set; } = RequestStatus.Draft;

    public Guid WorkflowDefinitionId { get; set; }
    public WorkflowDefinition WorkflowDefinition { get; set; } = default!;

    public Guid? CurrentStepId { get; set; }
    public WorkflowStep? CurrentStep { get; set; }

    public Guid SubmittedById { get; set; }
    public User SubmittedBy { get; set; } = default!;

    // PostgreSQL xmin system column for Optimistic Concurrency
    public uint RowVersion { get; set; }

    public ICollection<ApprovalAction> Actions { get; set; } = new List<ApprovalAction>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
