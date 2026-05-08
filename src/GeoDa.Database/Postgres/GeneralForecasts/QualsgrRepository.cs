using GeoDa.Application.GeneralForecasts.Repository.Qualsgrs;
using GeoDa.Application.GeneralForecasts.Repository.Qualsgrs.Dtos;
using GeoDa.Application.Exceptions;
using GeoDa.Database.Postgres.GeneralForecasts.DbContexts;
using GeoDa.Domain.Models;
using GeoDa.Domain.Services.GeoDaUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Database.Postgres.GeneralForecasts;

internal class QualsgrRepository : Repository, IQualsgrRepository
{
    private readonly IGeoDaDomainUtils _geoUtils;
    private readonly ILogger<QualsgrRepository> _logger;

    public QualsgrRepository(
        IGeoDaDomainUtils geoUtils,
        ILogger<QualsgrRepository> logger)
    {
        _geoUtils = geoUtils;
        _logger = logger;
    }

    // IQualsgrRepository
    public List<QualsgrDto> SelectAllQualsgrsAtTimeRange(int objectId, DateTime start, DateTime end)
    {
        var startDate = _geoUtils.GetDateInAzonFormat(start);
        var startTime = _geoUtils.GetTimeInAzonFormat(start);

        var endDate = _geoUtils.GetDateInAzonFormat(end);
        var endTime = _geoUtils.GetTimeInAzonFormat(end);

        try
        {
            using var dbc = GeneralDbContextFactory.Create(DbConnectionString);

            var result = new List<QualsgrDto>();

            if (startDate == endDate)
            {
                var tmp = dbc.Qualsgrs
                        .Where(v => v.Obj == objectId)
                        .Where(v => v.Idat == startDate)
                        .Where(v => v.Itim >= startTime && v.Itim <= endTime)
                        .ToList();

                result.AddRange(tmp);
            }
            else if (startDate < endDate)
            {
                var tmp = dbc.Qualsgrs
                       .Where(v => v.Obj == objectId
                           && ((v.Idat == startDate && v.Itim >= startTime && v.Itim <= 235959)
                                || (v.Idat == endDate && v.Itim >= 0 && v.Itim <= endTime)
                                || (v.Idat > startDate && v.Idat < endDate)))
                       .ToList();

                result.AddRange(tmp);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.GetDataError, ex.Message);
        }
    }
}
