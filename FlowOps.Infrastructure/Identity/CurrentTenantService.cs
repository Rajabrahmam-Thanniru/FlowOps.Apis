using System;
using System.Security.Claims;
using FlowOps.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FlowOps.Infrastructure.Identity;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : null;
        }
    }

    public Guid? UserId
    {
        get
        {
            var userClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userClaim, out var userId) ? userId : null;
        }
    }
}
