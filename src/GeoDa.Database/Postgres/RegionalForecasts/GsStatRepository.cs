using GeoDa.Application.RegionalForecasts.Repository.GsStats;
using GeoDa.Application.RegionalForecasts.Repository.GsStats.Dtos;
using GeoDa.Application.Exceptions;
using GeoDa.Database.Postgres.RegionalForecasts.DbContexts;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;

namespace GeoDa.Database.Postgres.RegionalForecasts;

internal class GsStatRepository : Repository, IGsStatRepository
{
    private readonly ILogger<GsStatRepository> _logger;

    public GsStatRepository(ILogger<GsStatRepository> logger)
    {
        _logger = logger;
    }

    // IGsStatRepository
    public GsStatDto InsertGsStat(GsStatDto gsStat)
    {
        try
        {
            using var dbc = RegionalForecastDbContextFactory.Create(DbConnectionString);

            dbc.GsStats.Add(gsStat);

            dbc.SaveChanges();

            return gsStat;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.InsertDataError, ex.Message);
        }
    }
}
