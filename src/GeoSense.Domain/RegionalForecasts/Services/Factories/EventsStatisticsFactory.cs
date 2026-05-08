using GeoDa.Domain.RegionalForecasts.Models;
using System;

namespace GeoDa.Domain.RegionalForecasts.Services.Factories;

internal class EventsStatisticsFactory : IEventsStatisticsFactory
{
    public EventsStatistics CreateDefault() =>
        new(MinVal: 0,
            MaxVal: 0,
            Q70: 0,
            Q80: 0,
            Q90: 0,
            Q95: 0,
            Q99: 0,
            NRows: 0,
            DateTimeOfOldestEvent: DateTime.MinValue,
            DateTimeOfNewestEvent: DateTime.MinValue);
}
