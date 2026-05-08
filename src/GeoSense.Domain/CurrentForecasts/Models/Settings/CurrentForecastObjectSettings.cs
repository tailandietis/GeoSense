namespace GeoDa.Domain.CurrentForecasts.Models.Settings;

public class CurrentForecastObjectSettings
{
    public double DSliceLL { get; set; }
    public double DSliceHH { get; set; }
    public double DBlockLL { get; set; }
    public double BlockX0 { get; set; }
    public double BlockX1 { get; set; }
    public double BlockY0 { get; set; }
    public double BlockY1 { get; set; }
    public double MaxDValue { get; set; }
    public int HelpMethodVersion { get; set; }
    public int MaxMShtbs { get; set; }
}
