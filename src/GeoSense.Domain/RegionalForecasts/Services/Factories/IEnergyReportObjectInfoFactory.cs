using GeoDa.Domain.RegionalForecasts.Models;
using System;

namespace GeoDa.Domain.RegionalForecasts.Services.Factories;

public interface IEnergyReportObjectInfoFactory
{
    public EnergyReportObjectInfo Create(
        int objectId,
        string objectName,
        DateTime dateTimeOfAssessment,
        double energyLimit,
        int checkIntervalInDays,
        double mineMapWidth,
        double mineMapHeight,
        int countOfBlockAtWidth,
        int countOfBlockAtHeight);

    public EnergyReportObjectInfo CreateDefault();
}
