namespace GeoDa.Domain.AlarmMonitors.Models.Factories;

internal static class AlarmIdModelFactory
{
    public static AlarmId Create(int objectId, AlarmCode alarmCode) =>
        new(ObjectId: objectId, AlarmCode: alarmCode);
}
