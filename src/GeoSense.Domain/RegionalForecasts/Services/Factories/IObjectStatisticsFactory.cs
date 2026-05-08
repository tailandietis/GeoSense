using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using System;

namespace GeoDa.Domain.RegionalForecasts.Services.Factories;

public interface IObjectStatisticsFactory
{
    public ObjectStatistics Create(int objectId,
        DateTime dateTimeOfStatCalc,
        Quality quality,
        EventsStatistics statistics,
        int statCalcInterval);

    public ObjectStatistics CreateDefault();
}
