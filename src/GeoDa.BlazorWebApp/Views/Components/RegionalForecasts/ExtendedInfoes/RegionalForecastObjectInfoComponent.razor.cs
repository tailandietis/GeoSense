using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Models.Settings.ParamFBlockSettings;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace GeoDa.BlazorWebApp.Views.Components.RegionalForecasts.ExtendedInfoes;

public partial class RegionalForecastObjectInfoComponent
{
    [Parameter]
    public string ObjectName { get; set; } = string.Empty;

    [Parameter]
    public (ObjectStatus ObjectStatus, EnergyAssessment Data) EnergyAssessment { get; set; } = new();

    [Parameter]
    public (ObjectStatus ObjectStatus, List<Event> Data) AlarmEvents { get; set; } = new();

    [Parameter]
    public (ObjectStatus ObjectStatus, GpnsCountInfo Data) GpnsCountInfo { get; set; } = new();

    [Parameter]
    public (ObjectStatus ObjectStatus, ObjectStatistics Data) Statistics { get; set; } = new();

    [Parameter]
    public (ObjectStatus ObjectStatus, RegionalForecastBlocksInfo Data) RfBlocksInfo { get; set; } = new();

    [Parameter]
    public RfParamFBlockSettings ParamFBlockSettings { get; set; } = default!;

    [Parameter]
    public string DensityEventsMapImagePath { get; set; } = string.Empty;

    [Parameter]
    public string DensityEventsWithLimitMapImagePath { get; set; } = string.Empty;

    [Parameter]
    public string DensityEnergyMapImagePath { get; set; } = string.Empty;

    [Parameter]
    public string ParamFMapImagePath { get; set; } = string.Empty;
    
    [Parameter]
    public string CurrentEnergyMapImagePath { get; set; } = string.Empty;

    // Private
    private string GetTitle() =>
        "Региональный прогноз";

    private string GetObjectNameLabel() =>
        $"Объект: {ObjectName}";

    // Maps
    private string GetDensityEventsMapTitle() =>
        "Карта плотности событий";
        
    private string GetDensityEventsMapImagePath()
    {
        var result = DensityEventsMapImagePath + "?DummyId=" + DateTime.Now.Ticks;

        return result;
    }

    private string GetDensityEventsWithLimitMapTitle() =>
        "Карта плотности событий с ограничением по порогу";

    private string GetDensityEventsWithLimitMapImagePath()
    {
        var result = DensityEventsWithLimitMapImagePath + "?DummyId=" + DateTime.Now.Ticks;

        return result;
    }

    private string GetDensityEnergyMapTitle() =>
        "Карта плотности энергий";

    private string GetDensityEnergyMapImagePath()
    {
        var result = DensityEnergyMapImagePath + "?DummyId=" + DateTime.Now.Ticks;

        return result;
    }

    private string GetParamFMapTitle() =>
        "Карта распределения значений параметра F за период";

    private string GetParamFMapImagePath()
    {
        var result = ParamFMapImagePath + "?DummyId=" + DateTime.Now.Ticks;

        return result;
    }

    private string GetCurrentEnergyMapTitle() =>
        "Карта распределения значений параметра приведенной энергии за период";

    private string GetCurrentEnergyMapImagePath()
    {
        var result = CurrentEnergyMapImagePath + "?DummyId=" + DateTime.Now.Ticks;

        return result;
    }
}
