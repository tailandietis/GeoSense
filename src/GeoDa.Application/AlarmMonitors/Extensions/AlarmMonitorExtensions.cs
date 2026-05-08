using GeoDa.Application.AlarmMonitors.Models;
using GeoDa.Application.AlarmMonitors.Repository.AlarmItems.Dtos;
using GeoDa.Domain.AlarmMonitors.Models;
using GeoDa.Domain.AlarmMonitors.Services;

namespace GeoDa.Application.AlarmMonitors.Extensions;

internal static class AlarmMonitorExtensions
{
    public static AlarmInfo ToAlarmInfo(
        this AlarmItemDto alarm,
        string objectName,
        IAlarmCodeFactory alarmCodeFactory,
        IAlarmIdFactory alarmIdFactory)
    {
        var result = new AlarmInfo(
            AlarmId: alarmIdFactory.Create(alarm.ObjectId,
                alarmCodeFactory.Create(alarm.ModuleFamilyCode, alarm.ServiceFamilyCode, alarm.ErrorCode)),
            ObjectName: objectName,
            DateTime: alarm.Dt,
            AlarmStatus: (AlarmStatus)(alarm.Status));

        return result;
    }

    public static AlarmItemDto ToAlarmItemDto(this AlarmInfo alarm)
    {
        var result = new AlarmItemDto()
        {
            ModuleFamilyCode = alarm.AlarmId.AlarmCode.ModuleFamilyCode,
            ObjectId = alarm.AlarmId.ObjectId,
            ServiceFamilyCode = alarm.AlarmId.AlarmCode.ServiceFamilyCode,
            ErrorCode = alarm.AlarmId.AlarmCode.ErrorCode,
            Status = (int)alarm.AlarmStatus,
            Dt = alarm.DateTime
        };

        return result;
    }
}
