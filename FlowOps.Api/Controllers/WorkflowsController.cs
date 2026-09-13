using System;
using System.Threading;
using System.Threading.Tasks;
using FlowOps.Application.Common.Interfaces;
using FlowOps.Application.Dtos.Workflows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowOps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;

    public WorkflowsController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkflows(CancellationToken cancellationToken)
    {
        var result = await _workflowService.GetWorkflowsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetWorkflow(Guid id, CancellationToken cancellationToken)
    {
        var result = await _workflowService.GetWorkflowByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkflow([FromBody] CreateWorkflowDto request, CancellationToken cancellationToken)
    {
        var result = await _workflowService.CreateWorkflowAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetWorkflow), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        await _workflowService.ToggleWorkflowActiveStatusAsync(id, cancellationToken);
        return NoContent();
    }
}
