using GeoDa.Application.Exceptions;
using GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies;
using GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies.Dtos;
using GeoDa.Application.RegionalForecasts.Repository.GsStats;
using GeoDa.Application.RegionalForecasts.Repository.GsStats.Dtos;
using GeoDa.Database.Postgres.RegionalForecasts.DbContexts;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;

namespace GeoDa.Database.Postgres.RegionalForecasts;

internal class GsParamFEnergyRepository : Repository, IGsParamFEnergyRepository
{
    private readonly ILogger<GsParamFEnergyRepository> _logger;

    public GsParamFEnergyRepository(ILogger<GsParamFEnergyRepository> logger)
    {
        _logger = logger;
    }

    // GsParamFEnergyRepository
    public void InsertGsParamFEnergy(GsParamFEnergyDto value)
    {
        try
        {
            using var dbc = RegionalForecastDbContextFactory.Create(DbConnectionString);

            dbc.GsParamFEnergy.Add(value);

            dbc.SaveChanges();            
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.InsertDataError, ex.Message);
        }
    }
}
