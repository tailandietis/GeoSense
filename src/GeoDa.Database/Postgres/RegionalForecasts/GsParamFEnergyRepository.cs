using GeoDa.Application.Exceptions;
using GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies;
using GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies.Dtos;
using GeoDa.Database.Postgres.RegionalForecasts.DbContexts;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Database.Postgres.RegionalForecasts;

internal class GsParamFEnergyRepository : Repository, IGsParamFEnergyRepository
{
    private readonly ILogger<GsParamFEnergyRepository> _logger;

    public GsParamFEnergyRepository(ILogger<GsParamFEnergyRepository> logger)
    {
        _logger = logger;
    }

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

    public List<GsParamFEnergyDto> SelectGsParamFEnergy(int objId, DateTime start, DateTime end)
    {
        try
        {
            using var dbc = RegionalForecastDbContextFactory.Create(DbConnectionString);
            return dbc.GsParamFEnergy
                .Where(x => x.Obj == objId && x.Dt >= start && x.Dt <= end)
                .OrderBy(x => x.Dt)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);
            return new List<GsParamFEnergyDto>();
        }
    }
}
