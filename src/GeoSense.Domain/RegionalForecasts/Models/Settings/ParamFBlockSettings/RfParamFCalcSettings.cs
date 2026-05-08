using System;

namespace GeoDa.Domain.RegionalForecasts.Models.Settings.ParamFBlockSettings;

public class RfParamFCalcSettings
{
    public double BlockSize { get; set; }

    public int TimeSpanMinImpactInDays { get; set; }

    public int TimeSpanRegInDays { get; set; }

    public double BackgroundEnergyValue { get; set; }
}