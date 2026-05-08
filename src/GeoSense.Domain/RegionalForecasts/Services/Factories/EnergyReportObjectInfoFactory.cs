using GeoDa.Domain.RegionalForecasts.Models;
using System;

namespace GeoDa.Domain.RegionalForecasts.Services.Factories;

public class EnergyReportObjectInfoFactory : IEnergyReportObjectInfoFactory
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
        int countOfBlockAtHeight) =>
        new(ObjectId: objectId,
            ObjectName: objectName,
            DateTimeOfAssessment: dateTimeOfAssessment,
            EnergyLimit: energyLimit,
            CheckIntervalInDays: checkIntervalInDays,
            MineMapWidth: mineMapWidth,
            MineMapHeight: mineMapHeight,
            CountOfBlockAtWidth: countOfBlockAtWidth,
            CountOfBlockAtHeight: countOfBlockAtHeight);

    public EnergyReportObjectInfo CreateDefault() =>
        new(ObjectId: -1,
            ObjectName: string.Empty,
            DateTimeOfAssessment: DateTime.MinValue,
            EnergyLimit: 0,
            CheckIntervalInDays: 0,
            MineMapWidth: 0,
            MineMapHeight: 0,
            CountOfBlockAtWidth: 0,
            CountOfBlockAtHeight: 0);
}
