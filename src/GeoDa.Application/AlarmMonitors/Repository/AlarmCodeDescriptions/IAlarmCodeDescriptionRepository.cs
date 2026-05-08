using GeoDa.Application.AlarmMonitors.Repository.AlarmCodeDescriptions.Dtos;
using GeoDa.Application.Databases;
using System.Collections.Generic;

namespace AlarmMonitor.Brokers.Storages.AlarmMonitorStorageBrokers;

public interface IAlarmCodeDescriptionRepository : IRepository
{
    List<AlarmCodeDescriptionDto> SelectAllAlarmCodeDescriptions();
}
