using GeoDa.Application.Exceptions;
using GeoDa.Application.GeneralForecasts.Repository.MsgCodeTexts;
using GeoDa.Application.GeneralForecasts.Repository.MsgCodeTexts.Dtos;
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

class MsgCodeTextRepository : Repository, IMsgCodeTextRepository
{
    private readonly ILogger<MsgCodeTextRepository> _logger;

    public MsgCodeTextRepository(ILogger<MsgCodeTextRepository> logger)
    {
        _logger = logger;
    }

    // IMsgCodeTextRepository
    public IReadOnlyList<MsgCodeTextDto> SelectAll()
    {
        try
        {
            using var dbc = GeneralDbContextFactory.Create(DbConnectionString);

            var result = dbc.MsgCodeTexts.ToList();            

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.GetDataError, ex.Message);
        }
    }
}
