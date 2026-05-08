using GeoDa.Domain.RegionalForecasts.Models;

namespace GeoDa.Domain.RegionalForecasts.Services.Factories;

public class EnergyAssessmentFactory : IEnergyAssessmentFactory
{
    private readonly IEnergyReportObjectInfoFactory _energyReportObjectInfoFactory;
    private readonly IEnergyReportFactory _energyReportFactory;

    public EnergyAssessmentFactory(
        IEnergyReportObjectInfoFactory energyReportObjectInfoFactory,
        IEnergyReportFactory energyReportFactory)
    {
        _energyReportObjectInfoFactory = energyReportObjectInfoFactory;
        _energyReportFactory = energyReportFactory;
    }

    public EnergyAssessment Create(
        EnergyReportObjectInfo energyReportObjectInfo,
        EnergyReport energyReport) =>
        new(Object: energyReportObjectInfo,
            Report: energyReport);

    public EnergyAssessment CreateDefault() =>
        new(Object: _energyReportObjectInfoFactory.CreateDefault(),
            Report: _energyReportFactory.CreateDefault());
}
