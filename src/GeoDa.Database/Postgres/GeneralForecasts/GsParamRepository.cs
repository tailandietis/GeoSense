using GeoDa.Application.GeneralForecasts.Repository.GsParams;
using GeoDa.Application.GeneralForecasts.Repository.GsParams.Dtos;
using GeoDa.Application.Exceptions;
using GeoDa.Database.Postgres.GeneralForecasts.DbContexts;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Database.Postgres.GeneralForecasts;

internal class GsParamRepository : Repository, IGsParamRepository
{
    private readonly ILogger<GsParamRepository> _logger;

    public GsParamRepository(ILogger<GsParamRepository> logger)
    {
        _logger = logger;
    }

    // IGsParamRepository
    public IReadOnlyList<GsParamDto> InsertGsParams(IReadOnlyList<GsParamDto> gsParams)
    {
        try
        {
            using var dbc = GeneralDbContextFactory.Create(DbConnectionString);

            dbc.GsParams.AddRange(gsParams.ToArray());

            dbc.SaveChanges();

            return gsParams.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.InsertDataError, ex.Message);
        }
    }
}
