using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowOps.Application.Common.Interfaces;
using FlowOps.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlowOps.Infrastructure.Jobs;

public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    public ReminderBackgroundService(IServiceProvider serviceProvider, ILogger<ReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderBackgroundService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("ReminderBackgroundService running check at: {time}", DateTimeOffset.Now);

            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing ReminderBackgroundService.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IFlowOpsDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var cutoffDate = DateTime.UtcNow.AddDays(-2);

        var stuckRequests = await dbContext.WorkflowRequests
            .IgnoreQueryFilters()
            .Include(r => r.CurrentStep)
            .Where(r => r.Status == RequestStatus.InReview && r.UpdatedAtUtc < cutoffDate)
            .ToListAsync(stoppingToken);

        foreach (var request in stuckRequests)
        {
            if (request.CurrentStep?.AssignedUserId != null)
            {
                var message = $"Reminder: Request '{request.Title}' ({request.RequestNumber}) has been pending your approval for over 2 days.";
                await notificationService.NotifyUserAsync(request.CurrentStep.AssignedUserId.Value, "Pending Approval Reminder", message, stoppingToken);
                _logger.LogInformation("Sent reminder to User {UserId} for Request {RequestId}", request.CurrentStep.AssignedUserId.Value, request.Id);
            }
        }
    }
}
