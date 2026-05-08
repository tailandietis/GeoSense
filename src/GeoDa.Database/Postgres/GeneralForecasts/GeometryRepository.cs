using GeoDa.Application.GeneralForecasts.Repository.Geometries;
using GeoDa.Application.GeneralForecasts.Repository.Geometries.Dtos;
using GeoDa.Application.Exceptions;
using GeoDa.Database.Postgres.GeneralForecasts.DbContexts;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace GeoDa.Database.Postgres.GeneralForecasts;

internal class GeometryRepository : Repository, IGeometryRepository
{
    private readonly ILogger<GeometryRepository> _logger;

    public GeometryRepository(ILogger<GeometryRepository> logger)
    {
        _logger = logger;
    }

    // IGeometryRepository
    public GeometryDto SelectGeometryData(int objectId)
    {
        try
        {
            using var dbc = GeneralDbContextFactory.Create(DbConnectionString);

            var result = dbc.Geometrs.Where(v => v.Obj == objectId).FirstOrDefault();

            if (result == null)
                throw new GeoDaAppException(OpStatus.NoData, $"Нет данных по геометрии для {objectId}");

            return result;
        }
        catch (GeoDaAppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.GetDataError, ex.Message);
        }
    }
}