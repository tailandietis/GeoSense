using GeoDa.Application.RegionalForecasts.Repository.Events;
using GeoDa.Application.RegionalForecasts.Repository.Events.Dtos;
using GeoDa.Application.Exceptions;
using GeoDa.Database.Postgres.RegionalForecasts.DbContexts;
using GeoDa.Domain.Models;
using GeoDa.Domain.Services.GeoDaUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Database.Postgres.RegionalForecasts;

internal class EventRepository : Repository, IEventRepository
{
    private readonly IGeoDaDomainUtils _geoUtils;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(
        IGeoDaDomainUtils geoUtils,
        ILogger<EventRepository> logger)
    {
        _geoUtils = geoUtils;
        _logger = logger;
    }

    // IEventRepository
    public List<EventDto> SelectEventsAtRangeGreaterOrEqualToEnergyLevel(
        int objId,
        DateTime start,
        DateTime end,
        double alarmLevel)
    {
        var azonStartDate = _geoUtils.GetDateInAzonFormat(start);
        var azonEndDate = _geoUtils.GetDateInAzonFormat(end);

        try
        {
            using var dbc = RegionalForecastDbContextFactory.Create(DbConnectionString);

            var result = dbc.Events
                .Where(v => v.Obj == objId)
                .Where(v => v.Idat >= azonStartDate && v.Idat <= azonEndDate)
                .Where(v => v.E >= alarmLevel)
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.GetDataError, ex.Message);
        }
    }

    public List<EventDto> SelectEventsAtRange(int objId, DateTime start, DateTime end)
    {
        var azonStartDate = _geoUtils.GetDateInAzonFormat(start);
        var azonEndDate = _geoUtils.GetDateInAzonFormat(end);

        try
        {
            using var dbc = RegionalForecastDbContextFactory.Create(DbConnectionString);

            var result = dbc.Events
                .Where(v => v.Obj == objId)
                .Where(v => v.Idat >= azonStartDate && v.Idat <= azonEndDate)
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.GetDataError, ex.Message);
        }
    }
}
