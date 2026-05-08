using GeoDa.Application.AlarmMonitors.Models;
using GeoDa.Domain.AlarmMonitors.Models;
using System;
using System.Collections.Generic;

namespace GeoDa.Application.AlarmMonitors.Services.AlarmMonitorServices;

public interface IAlarmMonitorService
{
    AlarmInfo BuildActiveAlarmInfo(string objectName, AlarmId alarmId);

    void AddAlarm(AlarmInfo alarmInfo);

    bool IsAlarmExists(AlarmId alarmId);

    bool IsAnyAlarmOccured();

    void RemoveActiveAlarm(AlarmId alarmId);

    void SetAlarmStatus(AlarmId alarmId, AlarmStatus alarmStatus);

    IReadOnlyList<AlarmInfo> GetAllActiveAlarms();

    string GetTextDescription(AlarmId alarmId);

    IReadOnlyList<AlarmInfo> GetAlarmsFromArchive(int moduleFamilyCode, int objectId, DateTime start, DateTime end);
}
