using GeoDa.Domain.RegionalForecasts.Models;
using System.Collections.Generic;

namespace GeoDa.Domain.RegionalForecasts.Services.StatCalcs;

public interface IStatCalcService
{
    EventsStatistics CalcEventsStat(IReadOnlyList<Event> events);
}
