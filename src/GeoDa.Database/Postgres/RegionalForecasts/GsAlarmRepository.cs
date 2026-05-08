using GeoDa.Application.RegionalForecasts.Repository.GsAlarms;
using GeoDa.Application.RegionalForecasts.Repository.GsAlarms.Dtos;
using GeoDa.Application.Exceptions;
using GeoDa.Database.Postgres.RegionalForecasts.DbContexts;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;

namespace GeoDa.Database.Postgres.RegionalForecasts;

internal class GsAlarmRepository : Repository, IGsAlarmRepository
{
    private readonly ILogger<GsAlarmRepository> _logger;

    public GsAlarmRepository(ILogger<GsAlarmRepository> logger)
    {
        _logger = logger;
    }

    // IGsAlarmRepository
    public GsAlarmDto InsertGsAlarm(GsAlarmDto gsAlarm)
    {
        try
        {
            using var dbc = RegionalForecastDbContextFactory.Create(DbConnectionString);

            dbc.GsAlarms.Add(gsAlarm);

            dbc.SaveChanges();

            return gsAlarm;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.InsertDataError, ex.Message);
        }
    }
}
