using System;
using FlowOps.Application.Common.Interfaces;

namespace FlowOps.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
