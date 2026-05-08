using GeoDa.Application.AlarmMonitors.Repository.AlarmItems.Dtos;
using GeoDa.Application.Exceptions;
using GeoDa.Database.Postgres;
using GeoDa.Database.Postgres.AlarmMonitors.DbContexts;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlarmMonitor.Brokers.Storages.AlarmMonitorStorageBrokers;

internal class AlarmItemRepository : Repository, IAlarmItemRepository
{
    private readonly ILogger<AlarmItemRepository> _logger;

    public AlarmItemRepository(ILogger<AlarmItemRepository> logger)
    {
        _logger = logger;
    }

    // IAlarmCodeDescriptionRepository
    public IReadOnlyList<AlarmItemDto> SelectAllAlarmItems()
    {
        try
        {
            using var dbc = AlarmMonitorDbContextFactory.Create(DbConnectionString);
            var result = dbc.Alarms.ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.GetDataError, ex.Message);
        }
    }

    public IReadOnlyList<AlarmItemDto> SelectAlarmItems(int moduleFamilyCode, int objectId, DateTime startDt, DateTime endDt)
    {
        try
        {
            using var dbc = AlarmMonitorDbContextFactory.Create(DbConnectionString);

            var result = dbc.Alarms.Where(v => v.ModuleFamilyCode == moduleFamilyCode
                        && v.ObjectId == objectId
                        && v.Dt >= startDt
                        && v.Dt <= endDt)
                        .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.GetDataError, ex.Message);
        }
    }

    public AlarmItemDto InsertAlarmItem(AlarmItemDto alarmItem)
    {
        try
        {
            using var dbc = AlarmMonitorDbContextFactory.Create(DbConnectionString);

            dbc.Alarms.Add(alarmItem);
            dbc.SaveChanges();

            return alarmItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.InsertDataError, ex.Message);
        }
    }
}
