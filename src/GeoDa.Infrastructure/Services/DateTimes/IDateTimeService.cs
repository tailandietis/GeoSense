using System;

namespace GeoDa.Infrastructure.Services.DateTimes;

public interface IDateTimeService
{
    DateTime GetCurrentDateTime();

    DateTime GetDefaultDateTime();

    int CountMillisecondsInSecond { get; }
}
