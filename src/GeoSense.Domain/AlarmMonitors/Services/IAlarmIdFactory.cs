using GeoDa.Domain.AlarmMonitors.Models;

namespace GeoDa.Domain.AlarmMonitors.Services;

public interface IAlarmIdFactory
{
    AlarmId Create(int objectId, AlarmCode alarmCode);
}
