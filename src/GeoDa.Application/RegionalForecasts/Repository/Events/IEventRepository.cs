using GeoDa.Application.Databases;
using GeoDa.Application.RegionalForecasts.Repository.Events.Dtos;
using System;
using System.Collections.Generic;

namespace GeoDa.Application.RegionalForecasts.Repository.Events;

public interface IEventRepository : IRepository
{
    List<EventDto> SelectEventsAtRange(int objId, DateTime start, DateTime end);

    List<EventDto> SelectEventsAtRangeGreaterOrEqualToEnergyLevel(
        int objId,
        DateTime start,
        DateTime end,
        double alarmLevel);
}
