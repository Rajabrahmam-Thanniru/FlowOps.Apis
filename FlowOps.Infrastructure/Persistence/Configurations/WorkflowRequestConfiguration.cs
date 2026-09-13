using FlowOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowOps.Infrastructure.Persistence.Configurations;

public class WorkflowRequestConfiguration : IEntityTypeConfiguration<WorkflowRequest>
{
    public void Configure(EntityTypeBuilder<WorkflowRequest> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RequestNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => new { r.TenantId, r.RequestNumber }).IsUnique();

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.FormPayloadJson)
            .HasColumnType("jsonb");

        // PostgreSQL Optimistic Concurrency Token
        builder.Property(r => r.RowVersion)
            .IsRowVersion();

        builder.HasOne(r => r.WorkflowDefinition)
            .WithMany(w => w.Requests)
            .HasForeignKey(r => r.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.SubmittedBy)
            .WithMany()
            .HasForeignKey(r => r.SubmittedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
