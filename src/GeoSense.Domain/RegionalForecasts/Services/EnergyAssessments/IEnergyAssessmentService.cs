using GeoDa.Domain.AlarmMonitors.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using System.Collections.Generic;

namespace GeoDa.Domain.RegionalForecasts.Services.EnergyAssessments;

public interface IEnergyAssessmentService
{
    AlarmId BuildEnergyAssessmentAlarmId(int objectId);

    EnergyReport GetEnergyReport(IReadOnlyList<Event> events);
}
