using GeoDa.Domain.Exceptions;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.Services.GeoDaUtils;
using MathNet.Numerics.Statistics;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Domain.RegionalForecasts.Services.StatCalcs;

internal class StatCalcService : IStatCalcService
{
    public const int Quantile70 = 70;
    public const int Quantile80 = 80;
    public const int Quantile90 = 90;
    public const int Quantile95 = 95;
    public const int Quantile99 = 99;

    private readonly IGeoDaDomainUtils _utils;
    private readonly ILogger<StatCalcService> _logger;

    public StatCalcService(
        IGeoDaDomainUtils utils,
        ILogger<StatCalcService> logger)
    {
        _utils = utils;
        _logger = logger;
    }

    // IStatCalcService
    public EventsStatistics CalcEventsStat(IReadOnlyList<Event> events)
    {
        if (events.Count == 0)
            throw new GeoDaDomainException(OpStatus.NoData, $"Events list is empty");

        var eVals = events.Select(x => (double)x.E);
        var dtItems = events.Select(x => x.Dt);

        var result = new EventsStatistics(
            MinVal: (float)eVals.Min(),
            MaxVal: (float)eVals.Max(),
            Q70: (float)Statistics.QuantileCustom(eVals, Quantile70 / 100.0, QuantileDefinition.R7),
            Q80: (float)Statistics.QuantileCustom(eVals, Quantile80 / 100.0, QuantileDefinition.R7),
            Q90: (float)Statistics.QuantileCustom(eVals, Quantile90 / 100.0, QuantileDefinition.R7),
            Q95: (float)Statistics.QuantileCustom(eVals, Quantile95 / 100.0, QuantileDefinition.R7),
            Q99: (float)Statistics.QuantileCustom(eVals, Quantile99 / 100.0, QuantileDefinition.R7),
            NRows: events.Count,
            DateTimeOfOldestEvent: dtItems.Min(),
            DateTimeOfNewestEvent: dtItems.Max());

        return result;
    }
}
