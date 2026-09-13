using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlowOps.Application.Common.Interfaces;

public interface INotificationService
{
    Task NotifyUserAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default);
    Task NotifyTenantAsync(Guid tenantId, string title, string message, CancellationToken cancellationToken = default);
}
