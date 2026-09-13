using System;
using System.Collections.Generic;

namespace FlowOps.Application.Dtos.Requests;

public class SubmitRequestDto
{
    public Guid WorkflowDefinitionId { get; set; }
    public string Title { get; set; } = default!;
    public string FormPayloadJson { get; set; } = "{}";
}

public class ProcessApprovalDto
{
    public string? Comment { get; set; }
    public uint RowVersion { get; set; } 
}

public class RejectRequestDto
{
    public string Reason { get; set; } = default!;
    public uint RowVersion { get; set; }
}

public class ReturnRequestDto
{
    public string Reason { get; set; } = default!;
    public uint RowVersion { get; set; }
}
