namespace GeoDa.Domain.RegionalForecasts.Models.Settings;

public class RegionalForecastGeneralSettings
{
    public int AlarmCheckUpdatePauseInSeconds { get; set; }
    public int StatCalcUpdatePauseInSeconds { get; set; }
    public int ParamFCalcUpdatePauseInSeconds { get; set; }
}
