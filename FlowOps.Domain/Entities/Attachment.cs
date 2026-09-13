using System;
using FlowOps.Domain.Common;

namespace FlowOps.Domain.Entities;

public class Attachment : BaseEntity
{
    public Guid WorkflowRequestId { get; set; }
    public WorkflowRequest WorkflowRequest { get; set; } = default!;

    public string FileName { get; set; } = default!;
    public string StoragePath { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long FileSizeBytes { get; set; }
}
