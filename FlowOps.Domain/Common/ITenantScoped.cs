namespace FlowOps.Domain.Common;

public interface ITenantScoped
{
    public Guid TenantId { get; set; }
}
