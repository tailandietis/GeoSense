using GeoDa.Domain.RegionalForecasts.Models.Settings.ParamFBlockSettings;
using System.Collections.Generic;

namespace GeoDa.Domain.RegionalForecasts.Models.Settings;

public class RegionalForecastSettings
{
    public RegionalForecastDbSettrings DbSettings { get; set; } = new();

    public Dictionary<string, RegionalForecastObjectSettings> ObjectsSettings { get; set; } = new();

    public RegionalForecastGeneralSettings GeneralSettings { get; set; } = new();
}
