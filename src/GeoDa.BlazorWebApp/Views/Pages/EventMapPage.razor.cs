using AntDesign;
using GeoDa.Application.RegionalForecasts.Services;
using GeoDa.Application.RegionalForecasts.Services.VolumeMaps;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class EventMapPage : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private IRegionalForecastService RfService { get; set; } = default!;
    [Inject] private IRfVolumeMapsService VolumeMapsService { get; set; } = default!;

    [Parameter] public string ObjectName { get; set; } = string.Empty;

    private List<CascaderNode> _objectNames = new();
    private DateTime?[] _dateRange = new DateTime?[2];
    private double? _minEnergy = null;
    private string? _generatedMapUrl;

    private bool _isGenerateButtonDisabled =>
        string.IsNullOrEmpty(ObjectName) ||
        _dateRange is not { Length: 2 } ||
        _dateRange[0] is null ||
        _dateRange[1] is null;

    private bool _isSaveButtonDisabled => string.IsNullOrEmpty(_generatedMapUrl);

    protected override void OnParametersSet()
    {
        var data = RfService.GetObjectInfoAndStatuses();
        _objectNames = data.Keys.Select(k => new CascaderNode { Value = k, Label = k }).ToList();
        base.OnParametersSet();
    }

    private void OnObjectNameChanged(CascaderNode[] nodes)
    {
        ObjectName = nodes.Length > 0 ? nodes[0].Value : string.Empty;
        _generatedMapUrl = null;
        StateHasChanged();
    }

    private async Task GenerateAndOpenMap()
    {
        if (_isGenerateButtonDisabled) return;

        var start = _dateRange[0]!.Value;
        var end   = _dateRange[1]!.Value;

        var (status, events) = RfService.GetEvents(ObjectName, start, end, _minEnergy ?? 0);

        if (status != ObjectStatus.Ok)
        {
            await JS.InvokeVoidAsync("alert", $"Ошибка получения данных. Статус: {status}");
            return;
        }

        if (events.Count == 0)
        {
            await JS.InvokeVoidAsync("alert", "Нет событий для построения карты");
            return;
        }

        var objectInfo = RfService.GetObjectInfoAndStatuses();
        if (!objectInfo.TryGetValue(ObjectName, out var objectData))
        {
            await JS.InvokeVoidAsync("alert", $"Объект '{ObjectName}' не найден");
            return;
        }

        var objectId = objectData.ObjectInfo.Id;

        var objectSettings = RfService.GetObjectSettings(ObjectName);
        var layouts = BuildVolumeLayouts(objectSettings);

        var mapUrl = VolumeMapsService.CreateVolumeMap(objectId, events, layouts);
        if (mapUrl is null)
        {
            await JS.InvokeVoidAsync("alert", "Ошибка при построении 3D-карты");
            return;
        }

        _generatedMapUrl = mapUrl;
        await JS.InvokeVoidAsync("open", _generatedMapUrl, "_blank");
    }

    private static List<VolumeLayoutConfig> BuildVolumeLayouts(RegionalForecastObjectSettings? settings)
    {
        var layouts = new List<VolumeLayoutConfig>();

        if (settings is null)
            return layouts;

        if (!string.IsNullOrEmpty(settings.VolumeLayoutOneFileName))
            layouts.Add(new VolumeLayoutConfig(
                FilePath:    Path.GetFullPath(settings.VolumeLayoutOneFileName),
                ZCoordinate: settings.VolumeLayoutOneZCoor,
                Opacity:     settings.VolumeLayoutOneOpacity));

        if (!string.IsNullOrEmpty(settings.VolumeLayoutTwoFileName))
            layouts.Add(new VolumeLayoutConfig(
                FilePath:    Path.GetFullPath(settings.VolumeLayoutTwoFileName),
                ZCoordinate: settings.VolumeLayoutTwoZCoor,
                Opacity:     settings.VolumeLayoutTwoOpacity));

        return layouts;
    }

    private async Task SaveMap()
    {
        if (_isSaveButtonDisabled) return;

        var dateFrom = _dateRange[0]!.Value.ToString("yyyy-MM-dd");
        var dateTo   = _dateRange[1]!.Value.ToString("yyyy-MM-dd");
        var fileName = $"{ObjectName}_{dateFrom}_{dateTo}_{(int)(_minEnergy ?? 0)}.html";

        await JS.InvokeVoidAsync("eval",
            $"var a=document.createElement('a');" +
            $"a.href='{_generatedMapUrl}';" +
            $"a.download='{fileName}';" +
            $"document.body.appendChild(a);a.click();document.body.removeChild(a);");
    }
}
