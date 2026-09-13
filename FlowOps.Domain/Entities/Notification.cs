using System;
using FlowOps.Domain.Common;

namespace FlowOps.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid RecipientId { get; set; }
    public User Recipient { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string DeepLinkUrl { get; set; } = default!;
    public bool IsRead { get; set; } = false;
}
