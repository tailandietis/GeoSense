using GeoDa.Application.GeneralForecasts.Repository.Qualgps;
using GeoDa.Application.GeneralForecasts.Repository.Qualgps.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace GeoDa.Database.Postgres.GeneralForecasts;

internal class QualgpRepository : Repository, IQualgpRepository
{
    private readonly ILogger<QualgpRepository> _logger;

    public QualgpRepository(ILogger<QualgpRepository> logger)
    {
        _logger = logger;
    }

    // IQualgpRepository
    public List<QualgpDto> SelectAllQualgpsAtTimeRange(int objId, DateTime start, DateTime end)
    {
        throw new NotImplementedException();
    }
}
