using GeoDa.Application.AlarmMonitors.Repository.AlarmCodeDescriptions.Dtos;
using GeoDa.Application.Exceptions;
using GeoDa.Application.Exceptions;
using GeoDa.Database.Postgres;
using GeoDa.Database.Postgres.AlarmMonitors.DbContexts;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlarmMonitor.Brokers.Storages.AlarmMonitorStorageBrokers;

internal class AlarmCodeDescriptionRepository : Repository, IAlarmCodeDescriptionRepository
{
    private readonly ILogger<AlarmCodeDescriptionRepository> _logger;
    public AlarmCodeDescriptionRepository(ILogger<AlarmCodeDescriptionRepository> logger)
    {
        _logger = logger;
    }

    // IAlarmCodeDescriptionRepository
    public List<AlarmCodeDescriptionDto> SelectAllAlarmCodeDescriptions()
    {
        try
        {
            using var dbc = AlarmMonitorDbContextFactory.Create(DbConnectionString);

            var result = dbc.AlarmsCodes.ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.GetDataError, ex.Message);
        }
    }
}
