using GeoDa.Application.AlarmMonitors.Models;
using GeoDa.Domain.AlarmMonitors.Models;
using System.Collections.Generic;

namespace GeoDa.Application.AlarmMonitors.Repository.ActiveAlarmsCaches;

public interface IActiveAlarmsStore
{
    bool IsAlarmExists(AlarmId alarmId);

    void AddAlarm(AlarmInfo alarmInfo);

    AlarmInfo GetAlarm(AlarmId alarmId);

    void RemoveAlarm(AlarmId alarmId);

    void UpdateAlarmStatus(AlarmId alarmId, AlarmStatus alarmStatus);

    IReadOnlyList<AlarmInfo> GetAllAlarms();

    bool IsAnyAlarm();
}
