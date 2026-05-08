using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using System;

namespace GeoDa.Domain.RegionalForecasts.Services.Factories;

public interface IEnergyReportFactory
{
    public EnergyReport Create(
        bool isAlarm,
        Quality quality,
        DateTime dateTimeOfItem,
        double energy);

    public EnergyReport CreateDefault();
}
