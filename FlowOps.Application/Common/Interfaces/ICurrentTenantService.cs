using System;

namespace FlowOps.Application.Common.Interfaces;

public interface ICurrentTenantService
{
    Guid? TenantId { get; }
    Guid? UserId { get; }
}
