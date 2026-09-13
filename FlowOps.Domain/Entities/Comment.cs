using System;
using FlowOps.Domain.Common;

namespace FlowOps.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid WorkflowRequestId { get; set; }
    public WorkflowRequest WorkflowRequest { get; set; } = default!;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = default!;

    public string Text { get; set; } = default!;
}
