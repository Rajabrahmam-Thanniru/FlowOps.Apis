using System;
using System.Threading;
using System.Threading.Tasks;
using FlowOps.Application.Common.Interfaces;
using FlowOps.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FlowOps.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyUserAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", title, message, cancellationToken);
    }

    public async Task NotifyTenantAsync(Guid tenantId, string title, string message, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"Tenant_{tenantId}").SendAsync("ReceiveNotification", title, message, cancellationToken);
    }
}
