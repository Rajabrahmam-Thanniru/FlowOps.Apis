using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FlowOps.Infrastructure.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantId))
        {
            Groups.AddToGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");
        }
        
        return base.OnConnectedAsync();
    }
}
