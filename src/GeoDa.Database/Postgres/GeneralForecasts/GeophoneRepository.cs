using GeoDa.Application.GeneralForecasts.Repository.Geophones;
using GeoDa.Application.GeneralForecasts.Repository.Geophones.Dtos;
using GeoDa.Application.Exceptions;
using GeoDa.Database.Postgres.GeneralForecasts.DbContexts;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Database.Postgres.GeneralForecasts;

internal class GeophoneRepository : Repository, IGeophoneRepository
{
    private readonly ILogger<GeophoneRepository> _logger;

    public GeophoneRepository(ILogger<GeophoneRepository> logger)
    {
        _logger = logger;
    }

    // IGeophonRepository
    public List<GeophoneDto> SelectAllGeophonsData(int objId)
    {
        try
        {
            using var dbc = GeneralDbContextFactory.Create(DbConnectionString);

            var result = dbc.Geophons.Where(v => v.Obj == objId).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.GetDataError, ex.Message);
        }
    }
}
