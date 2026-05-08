using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using System;

namespace GeoDa.Domain.RegionalForecasts.Services.Factories;

public class EnergyReportFactory : IEnergyReportFactory
{
    public EnergyReport Create(bool isAlarm,
        Quality quality,
        DateTime dateTimeOfItem,
        double energy) =>
        new(IsAlarm: isAlarm,
            Quality: quality,
            DateTimeOfItem: dateTimeOfItem,
            Energy: energy);

    public EnergyReport CreateDefault() =>
        new(IsAlarm: false,
            Quality: Quality.Uncertain,
            DateTimeOfItem: DateTime.MinValue,
            Energy: 0);
}
