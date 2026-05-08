using Microsoft.Extensions.Logging;
using System;

namespace GeoDa.Infrastructure.Services.DateTimes;

internal class DateTimeService : IDateTimeService
{
    private readonly ILogger<DateTimeService> _logger;

    public DateTimeService(ILogger<DateTimeService> logger)
    {
        _logger = logger;
    }

    public DateTime GetCurrentDateTime() =>
        DateTime.Now;

    public DateTime GetDefaultDateTime() =>
        DateTime.MinValue;

    public int CountMillisecondsInSecond { get => 1000; }
}
