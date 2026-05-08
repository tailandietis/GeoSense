using GeoDa.Domain.AlarmMonitors.Models;
using GeoDa.Domain.AlarmMonitors.Models.Factories;

namespace GeoDa.Domain.AlarmMonitors.Services;

internal class AlarmCodeFactory : IAlarmCodeFactory
{
    public AlarmCode Create(int moduleFamilyCode, int serviceFamilyCode, int errorCode) =>
        AlarmCodeModelFactory.Create(moduleFamilyCode, serviceFamilyCode, errorCode);
}
