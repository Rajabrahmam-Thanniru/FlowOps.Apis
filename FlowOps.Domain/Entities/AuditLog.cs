using System;
using FlowOps.Domain.Common;

namespace FlowOps.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Action { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string ChangesJson { get; set; } = "{}";
}
