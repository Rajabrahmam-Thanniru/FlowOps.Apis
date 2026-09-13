using System;
using System.Threading;
using System.Threading.Tasks;
using FlowOps.Application.Dtos.Requests;

namespace FlowOps.Application.Common.Interfaces;

public interface IRequestEngine
{
    Task<Guid> SubmitRequestAsync(SubmitRequestDto request, CancellationToken cancellationToken = default);
    Task ApproveRequestAsync(Guid requestId, ProcessApprovalDto request, CancellationToken cancellationToken = default);
    Task RejectRequestAsync(Guid requestId, RejectRequestDto request, CancellationToken cancellationToken = default);
    Task ReturnRequestAsync(Guid requestId, ReturnRequestDto request, CancellationToken cancellationToken = default);
}
