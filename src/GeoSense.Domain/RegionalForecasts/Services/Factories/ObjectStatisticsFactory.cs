using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using System;

namespace GeoDa.Domain.RegionalForecasts.Services.Factories;

internal class ObjectStatisticsFactory : IObjectStatisticsFactory
{
    private readonly IEventsStatisticsFactory _eventsStatisticsFactory;

    public ObjectStatisticsFactory(IEventsStatisticsFactory eventsStatisticsFactory)
    {

        _eventsStatisticsFactory = eventsStatisticsFactory;

    }

    public ObjectStatistics Create(int objectId,
        DateTime dateTimeOfStatCalc,
        Quality quality,
        EventsStatistics statistics,
        int statCalcInterval) =>
        new(ObjectId: objectId,
            DateTimeOfStatCalc: dateTimeOfStatCalc,
            Quality: quality,
            Statistics: statistics,
            StatCalcInterval: statCalcInterval);

    public ObjectStatistics CreateDefault() =>
        new(ObjectId: -1,
            DateTimeOfStatCalc: DateTime.MinValue,
            Quality: Quality.Uncertain,
            Statistics: _eventsStatisticsFactory.CreateDefault(),
            StatCalcInterval: 0);
}
