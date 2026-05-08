namespace GeoDa.Domain.RegionalForecasts.Models.Settings.ParamFBlockSettings;

public record RfParamFBlockSettings
{
    public RfFieldSettings FieldSettings { get; set; }
    public RfParamFCalcSettings ParamFCalcSettings { get; set; }
    public double RadiusAddition { get; set; }
    public RfSeismoActivityEstimationSettings EstimationSettings { get; set; }
}