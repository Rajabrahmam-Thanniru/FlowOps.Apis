using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FlowOps.Application.Common.Interfaces;
using FlowOps.Application.Dtos.Requests;
using FlowOps.Application.Services;
using FlowOps.Domain.Entities;
using FlowOps.Domain.Enums;
using FlowOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FlowOps.UnitTests.Services;

public class RequestEngineTests
{
    private readonly DbContextOptions<FlowOpsDbContext> _options;

    public RequestEngineTests()
    {
        _options = new DbContextOptionsBuilder<FlowOpsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task SubmitRequestAsync_Should_CreateRequest_And_AuditLog()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workflowId = Guid.NewGuid();

        var mockTenantService = new Mock<ICurrentTenantService>();
        mockTenantService.Setup(x => x.TenantId).Returns(tenantId);
        mockTenantService.Setup(x => x.UserId).Returns(userId);

        var mockNotificationService = new Mock<INotificationService>();

        using (var context = new FlowOpsDbContext(_options, mockTenantService.Object))
        {
            var workflow = new WorkflowDefinition
            {
                Id = workflowId,
                Name = "Test Workflow",
                Description = "Test Description",
                IsActive = true,
                TenantId = tenantId
            };
            workflow.Steps.Add(new WorkflowStep
            {
                Name = "Step 1",
                StepOrder = 1,
                AssignmentType = StepAssignmentType.User,
                AssignedUserId = userId
            });
            context.WorkflowDefinitions.Add(workflow);
            await context.SaveChangesAsync();
        }

        using (var context = new FlowOpsDbContext(_options, mockTenantService.Object))
        {
            var engine = new RequestEngine(context, mockTenantService.Object, mockNotificationService.Object);
            var dto = new SubmitRequestDto
            {
                WorkflowDefinitionId = workflowId,
                Title = "Test Request",
                FormPayloadJson = "{}"
            };

            // Act
            var requestId = await engine.SubmitRequestAsync(dto);

            // Assert
            requestId.Should().NotBeEmpty();

            var request = await context.WorkflowRequests.FindAsync(requestId);
            request.Should().NotBeNull();
            request!.Title.Should().Be("Test Request");
            request.Status.Should().Be(RequestStatus.InReview);

            var auditLog = await context.AuditLogs.FirstOrDefaultAsync(a => a.EntityId == requestId.ToString());
            auditLog.Should().NotBeNull();
            auditLog!.Action.Should().Be("Submitted");
            
            mockNotificationService.Verify(x => x.NotifyTenantAsync(tenantId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
