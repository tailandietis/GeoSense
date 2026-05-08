using GeoDa.Domain.AlarmMonitors.Models;
using System;

namespace GeoDa.Application.AlarmMonitors.Models.Factories;

public static class AlarmInfoFactory
{
    public static AlarmInfo Create(AlarmId alarmId, string objectName, DateTime dateTime, AlarmStatus alarmStatus) =>
        new(AlarmId: alarmId, ObjectName: objectName, DateTime: dateTime, AlarmStatus: alarmStatus);
}
