using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowOps.Application.Common.Interfaces;
using FlowOps.Application.Dtos.Requests;
using FlowOps.Domain.Entities;
using FlowOps.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlowOps.Application.Services;

public class RequestEngine : IRequestEngine
{
    private readonly IFlowOpsDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly INotificationService _notificationService;

    public RequestEngine(IFlowOpsDbContext context, ICurrentTenantService currentTenantService, INotificationService notificationService)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _notificationService = notificationService;
    }

    public async Task<Guid> SubmitRequestAsync(SubmitRequestDto request, CancellationToken cancellationToken = default)
    {
        var workflow = await _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowDefinitionId, cancellationToken);

        if (workflow == null || !workflow.IsActive)
            throw new Exception("Workflow is invalid or inactive.");

        var firstStep = workflow.Steps.OrderBy(s => s.StepOrder).FirstOrDefault();
        if (firstStep == null)
            throw new Exception("Workflow has no steps configured.");

        var newRequest = new WorkflowRequest
        {
            WorkflowDefinitionId = workflow.Id,
            Title = request.Title,
            FormPayloadJson = request.FormPayloadJson,
            Status = RequestStatus.InReview,
            CurrentStepId = firstStep.Id,
            SubmittedById = _currentTenantService.UserId ?? throw new UnauthorizedAccessException(),
            RequestNumber = $"REQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}"
        };

        _context.WorkflowRequests.Add(newRequest);
        
        var audit = new AuditLog
        {
            Action = "Submitted",
            EntityName = nameof(WorkflowRequest),
            EntityId = newRequest.Id.ToString(),
            UserId = newRequest.SubmittedById
        };
        _context.AuditLogs.Add(audit);

        await _context.SaveChangesAsync(cancellationToken);

        if (_currentTenantService.TenantId.HasValue)
        {
            await _notificationService.NotifyTenantAsync(_currentTenantService.TenantId.Value, "New Request", $"Request {newRequest.RequestNumber} was submitted for approval.", cancellationToken);
        }

        return newRequest.Id;
    }

    public async Task ApproveRequestAsync(Guid requestId, ProcessApprovalDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentTenantService.UserId ?? throw new UnauthorizedAccessException();

        var wfRequest = await _context.WorkflowRequests
            .Include(r => r.WorkflowDefinition)
            .ThenInclude(w => w.Steps)
            .Include(r => r.CurrentStep)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (wfRequest == null) throw new Exception("Request not found.");
        if (wfRequest.Status != RequestStatus.InReview) throw new Exception("Request is not in review.");
        if (wfRequest.RowVersion != request.RowVersion) throw new DbUpdateConcurrencyException("Request was modified by another user.");

        var currentStepOrder = wfRequest.CurrentStep!.StepOrder;
        var nextStep = wfRequest.WorkflowDefinition.Steps
            .Where(s => s.StepOrder > currentStepOrder)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();

        var completedStepId = wfRequest.CurrentStepId.Value;

        if (nextStep == null)
        {
            wfRequest.Status = RequestStatus.Approved;
            wfRequest.CurrentStepId = null;
        }
        else
        {
            wfRequest.CurrentStepId = nextStep.Id;
        }

        var action = new ApprovalAction
        {
            WorkflowRequestId = wfRequest.Id,
            StepId = completedStepId,
            PerformedById = currentUserId,
            Action = ActionType.Approved,
            Reason = request.Comment
        };

        _context.ApprovalActions.Add(action);
        wfRequest.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectRequestAsync(Guid requestId, RejectRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentTenantService.UserId ?? throw new UnauthorizedAccessException();
        
        var wfRequest = await _context.WorkflowRequests
            .Include(r => r.CurrentStep)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (wfRequest == null) throw new Exception("Request not found.");
        if (wfRequest.Status != RequestStatus.InReview) throw new Exception("Request is not in review.");
        if (wfRequest.RowVersion != request.RowVersion) throw new DbUpdateConcurrencyException();

        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 10)
            throw new Exception("Rejection requires a reason of at least 10 characters.");

        var completedStepId = wfRequest.CurrentStepId!.Value;
        wfRequest.Status = RequestStatus.Rejected;
        wfRequest.CurrentStepId = null;
        wfRequest.UpdatedAtUtc = DateTime.UtcNow;

        var action = new ApprovalAction
        {
            WorkflowRequestId = wfRequest.Id,
            StepId = completedStepId,
            PerformedById = currentUserId,
            Action = ActionType.Rejected,
            Reason = request.Reason
        };

        _context.ApprovalActions.Add(action);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReturnRequestAsync(Guid requestId, ReturnRequestDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentTenantService.UserId ?? throw new UnauthorizedAccessException();
        
        var wfRequest = await _context.WorkflowRequests
            .Include(r => r.CurrentStep)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (wfRequest == null) throw new Exception("Request not found.");
        if (wfRequest.Status != RequestStatus.InReview) throw new Exception("Request is not in review.");
        if (wfRequest.RowVersion != request.RowVersion) throw new DbUpdateConcurrencyException();

        var completedStepId = wfRequest.CurrentStepId!.Value;
        wfRequest.Status = RequestStatus.Returned;
        wfRequest.CurrentStepId = null; 
        wfRequest.UpdatedAtUtc = DateTime.UtcNow;

        var action = new ApprovalAction
        {
            WorkflowRequestId = wfRequest.Id,
            StepId = completedStepId,
            PerformedById = currentUserId,
            Action = ActionType.Returned,
            Reason = request.Reason
        };

        _context.ApprovalActions.Add(action);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
