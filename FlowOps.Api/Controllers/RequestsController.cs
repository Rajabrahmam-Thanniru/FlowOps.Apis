using System;
using System.Threading;
using System.Threading.Tasks;
using FlowOps.Application.Common.Interfaces;
using FlowOps.Application.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowOps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly IRequestEngine _requestEngine;

    public RequestsController(IRequestEngine requestEngine)
    {
        _requestEngine = requestEngine;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitRequest([FromBody] SubmitRequestDto request, CancellationToken cancellationToken)
    {
        var id = await _requestEngine.SubmitRequestAsync(request, cancellationToken);
        return Ok(new { RequestId = id });
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveRequest(Guid id, [FromBody] ProcessApprovalDto request, CancellationToken cancellationToken)
    {
        await _requestEngine.ApproveRequestAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectRequest(Guid id, [FromBody] RejectRequestDto request, CancellationToken cancellationToken)
    {
        await _requestEngine.RejectRequestAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> ReturnRequest(Guid id, [FromBody] ReturnRequestDto request, CancellationToken cancellationToken)
    {
        await _requestEngine.ReturnRequestAsync(id, request, cancellationToken);
        return Ok();
    }
}
