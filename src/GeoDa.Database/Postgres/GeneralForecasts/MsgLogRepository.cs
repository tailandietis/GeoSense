using GeoDa.Application.Exceptions;
using GeoDa.Application.GeneralForecasts.Repository.MsgLogs;
using GeoDa.Application.GeneralForecasts.Repository.MsgLogs.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.Qualsgrs.Dtos;
using GeoDa.Database.Postgres.GeneralForecasts.DbContexts;
using GeoDa.Domain.Models;
using GeoDa.Domain.Services.GeoDaUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoDa.Database.Postgres.GeneralForecasts;

class MsgLogRepository : Repository, IMsgLogRepository
{
    private readonly IGeoDaDomainUtils _geoUtils;
    private readonly ILogger<MsgLogRepository> _logger;

    public MsgLogRepository(
        IGeoDaDomainUtils geoUtils,
        ILogger<MsgLogRepository> logger)
    {
        _geoUtils = geoUtils;
        _logger = logger;
    }

    // IMsgLogRepository
    public IReadOnlyList<MsgLogDto> SelectMessages(int objectId, DateTime start, DateTime end)
    {
        var startDate = _geoUtils.GetDateInAzonFormat(start);
        var startTime = _geoUtils.GetTimeInAzonFormat(start);

        var endDate = _geoUtils.GetDateInAzonFormat(end);
        var endTime = _geoUtils.GetTimeInAzonFormat(end);

        try
        {
            using var dbc = GeneralDbContextFactory.Create(DbConnectionString);

            var result = new List<MsgLogDto>();

            if (startDate == endDate)
            {
                var tmp = dbc.MsgLogs
                        .Where(v => v.ObjectId == objectId
                            && (v.Date == startDate && v.Time >= startTime && v.Time <= endTime))
                        .ToList();

                result.AddRange(tmp);
            }
            else if (startDate < endDate)
            {
                var tmp = dbc.MsgLogs
                       .Where(v => v.ObjectId == objectId
                           && ((v.Date == startDate && v.Time >= startTime && v.Time <= 235959)
                                || (v.Date == endDate && v.Time >= 0 && v.Time <= endTime)
                                || (v.Date > startDate && v.Date < endDate)))
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
