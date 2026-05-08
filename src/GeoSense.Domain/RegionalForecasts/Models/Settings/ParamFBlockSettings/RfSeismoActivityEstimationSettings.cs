namespace GeoDa.Domain.RegionalForecasts.Models.Settings.ParamFBlockSettings;

public record RfSeismoActivityEstimationSettings
{
    public double FBackgound { get; set; }
    public double ECurrentBackground { get; set; }
    public double FLevel1 { get; set; }
    public double ECurrentLevel1 { get; set; }
    public double FLevel2 { get; set; }
    public double ECurrentLevel2 { get; set; }
    public double FLevel3 { get; set; }
    public double ECurrentLevel3 { get; set; }
}