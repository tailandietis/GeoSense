using GeoDa.Domain.AlarmMonitors.Models;

namespace GeoDa.Domain.AlarmMonitors.Services;

public interface IAlarmCodeFactory
{
    AlarmCode Create(int moduleFamilyCode, int serviceFamilyCode, int errorCode);
}
