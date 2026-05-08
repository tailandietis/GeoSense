using GeoDa.Domain.AlarmMonitors.Models;
using GeoDa.Domain.AlarmMonitors.Models.Factories;

namespace GeoDa.Domain.AlarmMonitors.Services;

internal class AlarmIdFactory : IAlarmIdFactory
{
    public AlarmId Create(int objectId, AlarmCode alarmCode) =>
        AlarmIdModelFactory.Create(objectId, alarmCode);
}
