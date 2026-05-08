namespace GeoDa.Domain.RegionalForecasts.Models.Settings.ParamFBlockSettings;

public record RfFieldSettings
{
    public RfFieldCoords FieldCoords { get; set; } = new();
    public RfFieldSize FieldSize { get; set; } = new();
}