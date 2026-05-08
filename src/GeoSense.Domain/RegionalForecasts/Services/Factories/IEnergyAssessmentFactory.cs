using GeoDa.Domain.RegionalForecasts.Models;

namespace GeoDa.Domain.RegionalForecasts.Services.Factories;

public interface IEnergyAssessmentFactory
{
    public EnergyAssessment Create(
        EnergyReportObjectInfo energyReportObjectInfo,
        EnergyReport energyReport);

    public EnergyAssessment CreateDefault();
}
