using GeoDa.Domain.RegionalForecasts.Models.Settings.ParamFBlockSettings;

namespace GeoDa.Domain.RegionalForecasts.Models.Settings;

public class RegionalForecastObjectSettings
{   
    public int CountOfBlockAtWidth { get; set; }

    public int CountOfBlockAtHeight { get; set; }

    public string LayoutFileName { get; set; } = string.Empty;

    public string VolumeLayoutOneFileName { get; set; } = string.Empty;

    public int? VolumeLayoutOneZCoor { get; set; }

    public double VolumeLayoutOneOpacity { get; set; } = 1.0;

    public string VolumeLayoutTwoFileName { get; set; } = string.Empty;

    public int? VolumeLayoutTwoZCoor { get; set; }

    public double VolumeLayoutTwoOpacity { get; set; } = 1.0;

    public double AlarmELevel { get; set; }

    public int AlarmCheckIntervalInDays { get; set; }

    public int StatCalcIntervalInDays { get; set; }

    public RfParamFBlockSettings ParamFBlockSettings { get; set; } = new();
}
