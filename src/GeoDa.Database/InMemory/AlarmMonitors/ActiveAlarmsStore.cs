using GeoDa.Application.AlarmMonitors.Models;
using GeoDa.Application.AlarmMonitors.Repository.ActiveAlarmsCaches;
using GeoDa.Application.Exceptions;
using GeoDa.Domain.AlarmMonitors.Models;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Database.InMemory.AlarmMonitors;

internal class ActiveAlarmsStore : IActiveAlarmsStore
{
    private readonly ILogger<ActiveAlarmsStore> _logger;

    private readonly ConcurrentDictionary<AlarmId, AlarmInfo> _store = new();

    public ActiveAlarmsStore(ILogger<ActiveAlarmsStore> logger)
    {
        _logger = logger;
    }

    // IActiveAlarmsStore
    public bool IsAlarmExists(AlarmId alarmId)
    {
        var result = _store.ContainsKey(alarmId);

        return result;
    }

    public void AddAlarm(AlarmInfo alarmInfo)
    {
        if (_store.ContainsKey(alarmInfo.AlarmId) is false)
        {
            _store.AddOrUpdate(alarmInfo.AlarmId, alarmInfo, (k, v) => alarmInfo);
        }
    }

    public AlarmInfo GetAlarm(AlarmId alarmId)
    {
        if (_store.ContainsKey(alarmId) is false)
            throw new GeoDaAppException(OpStatus.NoData, $"AlarmId {alarmId} is absent in store");

        var result = _store[alarmId];

        return result;
    }

    public void RemoveAlarm(AlarmId alarmId)
    {
        _store.Remove(alarmId, out var tmp);
    }

    public void UpdateAlarmStatus(AlarmId alarmId, AlarmStatus alarmStatus)
    {
        if (_store.ContainsKey(alarmId))
            _store[alarmId] = _store[alarmId] with { AlarmStatus = alarmStatus };
    }

    public IReadOnlyList<AlarmInfo> GetAllAlarms()
    {
        var result = _store.Values
            .ToList()
            .AsReadOnly();

        return result;
    }

    public bool IsAnyAlarm() =>
        _store.Any();
}
