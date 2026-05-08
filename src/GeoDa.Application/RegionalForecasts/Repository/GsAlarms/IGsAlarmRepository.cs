using GeoDa.Application.Databases;
using GeoDa.Application.RegionalForecasts.Repository.GsAlarms.Dtos;

namespace GeoDa.Application.RegionalForecasts.Repository.GsAlarms;

public interface IGsAlarmRepository : IRepository
{
    GsAlarmDto InsertGsAlarm(GsAlarmDto gsAlarm);
}
