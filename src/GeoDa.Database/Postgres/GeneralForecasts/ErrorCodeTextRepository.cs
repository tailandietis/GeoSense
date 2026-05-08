using GeoDa.Application.Exceptions;
using GeoDa.Application.GeneralForecasts.Repository.ErrorCodeTexts;
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

class ErrorCodeTextRepository : Repository, IErrorCodeTextRepository
{
    private readonly ILogger<ErrorCodeTextRepository> _logger;

    public ErrorCodeTextRepository(ILogger<ErrorCodeTextRepository> logger)
    {
        _logger = logger;
    }

    // IErrorCodeTextRepository
    public IReadOnlyList<ErrorCodeTextDto> SelectAll()
    {
        try
        {
            using var dbc = GeneralDbContextFactory.Create(DbConnectionString);

            var result = dbc.ErrorCodeTexts.ToList();            

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.GetDataError, ex.Message);
        }
    }
}
