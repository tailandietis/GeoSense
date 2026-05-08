using GeoDa.Application.GeneralForecasts.Repository.ObjectInfos;
using GeoDa.Application.GeneralForecasts.Repository.ObjectInfos.Dtos;
using GeoDa.Application.Exceptions;
using GeoDa.Database.Postgres.GeneralForecasts.DbContexts;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Database.Postgres.GeneralForecasts;

internal class ObjectInfoRepository : Repository, IObjectInfoRepository
{
    private readonly ILogger<ObjectInfoRepository> _logger;

    public ObjectInfoRepository(ILogger<ObjectInfoRepository> logger)
    {
        _logger = logger;
    }

    // IObjectInfoRepository    
    public List<ObjectInfoDto> SelectAllObjectInfos()
    {
        try
        {
            using var dbc = GeneralDbContextFactory.Create(DbConnectionString);

            List<ObjectInfoDto> result = dbc.Objects
                .Select(x => Processing(x))
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.GetDataError, ex.Message);
        }
    }

    // Private
    private static ObjectInfoDto Processing(ObjectInfoDto objectInfoDto) =>
        new()
        {
            Obj = objectInfoDto.Obj,
            ObjTyp = objectInfoDto.ObjTyp,
            ObjName = objectInfoDto.ObjName.Trim(),
        };
}
