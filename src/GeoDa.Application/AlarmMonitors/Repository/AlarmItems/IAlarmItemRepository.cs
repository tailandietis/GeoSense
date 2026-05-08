using GeoDa.Application.AlarmMonitors.Repository.AlarmItems.Dtos;
using GeoDa.Application.Databases;
using System;
using System.Collections.Generic;

namespace AlarmMonitor.Brokers.Storages.AlarmMonitorStorageBrokers;

public interface IAlarmItemRepository : IRepository
{
    IReadOnlyList<AlarmItemDto> SelectAllAlarmItems();

    IReadOnlyList<AlarmItemDto> SelectAlarmItems(int moduleFamilyCode, int objectId, DateTime start, DateTime end);

    AlarmItemDto InsertAlarmItem(AlarmItemDto alarmItem);
}
