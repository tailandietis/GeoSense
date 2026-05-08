using GeoDa.Domain.AlarmMonitors.Models;
using GeoDa.Domain.AlarmMonitors.Services;
using GeoDa.Domain.Exceptions;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Services.Factories;
using GeoDa.Domain.Services.GeoDaUtils;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Domain.RegionalForecasts.Services.EnergyAssessments;

internal class EnergyAssessmentService : IEnergyAssessmentService
{
    private readonly IEnergyReportFactory _energyReportFactory;

    private readonly IAlarmCodeFactory _alarmCodeFactory;
    private readonly IAlarmIdFactory _alarmIdFactory;

    private readonly ILogger<EnergyAssessmentService> _logger;

    public EnergyAssessmentService(
        IEnergyReportFactory energyReportFactory,
        IAlarmCodeFactory alarmCodeFactory,
        IAlarmIdFactory alarmIdFactory,        
        ILogger<EnergyAssessmentService> logger)
    {
        _energyReportFactory = energyReportFactory;
        _alarmCodeFactory = alarmCodeFactory;
        _alarmIdFactory = alarmIdFactory;
        _logger = logger;
    }

    // IEnergyAssessmentService
    public AlarmId BuildEnergyAssessmentAlarmId(int objectId)
    {
        var alarmCode = _alarmCodeFactory.Create(moduleFamilyCode: RegionalForecastConsts.ModuleFamilyCode,
                serviceFamilyCode: RegionalForecastConsts.EnergyAssessmentServiceCode,
                errorCode: RegionalForecastConsts.EnergyAssessmentAlarmCode);

        var result = _alarmIdFactory.Create(objectId, alarmCode);

        return result;
    }

    public EnergyReport GetEnergyReport(IReadOnlyList<Event> eventsWithAlarmValues)
    {
        if (eventsWithAlarmValues.Count == 0)
        {
            var result = _energyReportFactory.CreateDefault()
                with
            { Quality = Quality.Good };

            return result;
        }
        else
        {
            var newestEvent = GetMostNewestEvent(eventsWithAlarmValues);

            var result = _energyReportFactory.CreateDefault() with
            {
                IsAlarm = true,
                Quality = Quality.Good,
                DateTimeOfItem = newestEvent.Dt,
                Energy = newestEvent.E
            };

            return result;
        }
    }

    // Private

    private static Event GetMostNewestEvent(IReadOnlyList<Event> events)
    {
        if (events.Count == 0)
            throw new GeoDaDomainException(OpStatus.NoData, $"Events list is empty");

        var eventsList = events.ToList();
        eventsList.Sort(new EventDateTimeComparer());
        var result = eventsList.Last();

        return result;
    }
}
