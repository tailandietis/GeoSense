using System.Collections.Generic;

namespace GeoDa.Domain.CurrentForecasts.Models.Settings;

public class CurrentForecastSettings
{
    public CurrentForecastDbSettrings DbSettings { get; set; } = new();

    public Dictionary<string, CurrentForecastObjectSettings> ObjectsSettings { get; set; } = new();

    public CurrentForecastGeneralSettings GeneralSettings { get; set; } = new();
}
