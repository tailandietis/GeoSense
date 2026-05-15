using AntDesign;
using GeoDa.Application.GeneralForecasts.Services.Utils;
using GeoDa.Application.RegionalForecasts.Repository.Events;
using GeoDa.Application.RegionalForecasts.Repository.Events.Dtos;
using GeoDa.Application.RegionalForecasts.Services.VolumeMaps;
using GeoDa.BlazorWebApp.Services.MLBuilder;
using GeoDa.Domain.RegionalForecasts.Models;
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
    [Inject] private IRfVolumeMapsService VolumeMapsService { get; set; } = default!;
    [Inject] private MLObjectStateService ObjectState { get; set; } = default!;
    [Inject] private IEventRepository EventRepository { get; set; } = default!;
    [Inject] private IGeneralForecastUtilsService Utils { get; set; } = default!;

    [Parameter] public string ObjectName { get; set; } = string.Empty;

    private List<CascaderNode> _objectNames = new();
    private DateTime?[] _dateRange = new DateTime?[2];
    private double? _minEnergy = null;
    private string? _generatedMapUrl;

    private bool _isGenerateButtonDisabled =>
        ObjectState.SelectedObjId is null ||
        _dateRange is not { Length: 2 } ||
        _dateRange[0] is null ||
        _dateRange[1] is null;

    private bool _isSaveButtonDisabled => string.IsNullOrEmpty(_generatedMapUrl);

    protected override void OnParametersSet()
    {
        var objects = ObjectState.GetObjects();
        _objectNames = objects
            .Select(o => new CascaderNode { Value = o.Obj.ToString(), Label = o.ObjName.Trim() })
            .ToList();

        if (ObjectState.SelectedObjId is not null)
            ObjectName = ObjectState.SelectedObjId.Value.ToString();

        base.OnParametersSet();
    }

    private void OnObjectNameChanged(CascaderNode[] nodes)
    {
        if (nodes.Length > 0 && int.TryParse(nodes[0].Value, out var objId))
        {
            ObjectState.SelectedObjId = objId;
            ObjectState.SelectedObjName = nodes[0].Label;
            ObjectName = nodes[0].Value;
        }
        _generatedMapUrl = null;
        StateHasChanged();
    }

    private async Task GenerateAndOpenMap()
    {
        if (_isGenerateButtonDisabled) return;

        var objId = ObjectState.SelectedObjId!.Value;
        var start = _dateRange[0]!.Value;
        var end   = _dateRange[1]!.Value;

        EventRepository.DbConnectionString = Utils.BuildDbConnectionString("DbPeleng");
        var dtos = EventRepository.SelectEventsAtRange(objId, start, end);

        if (_minEnergy is > 0)
            dtos = dtos.Where(e => (e.E ?? 0) >= _minEnergy.Value).ToList();

        if (dtos.Count == 0)
        {
            await JS.InvokeVoidAsync("alert", "Нет событий для построения карты");
            return;
        }

        var events = dtos.Select(ToEvent).ToList();
        var layouts = new List<VolumeLayoutConfig>
        {
            new(Path.GetFullPath(@".\templates\location_plan.png"),  null, 1.0),
            new(Path.GetFullPath(@".\templates\location_plan_2.jpg"), -50, 0.5),
        };
        var mapUrl = VolumeMapsService.CreateVolumeMap(objId, events, layouts);

        if (mapUrl is null)
        {
            await JS.InvokeVoidAsync("alert", "Ошибка при построении 3D-карты");
            return;
        }

        _generatedMapUrl = mapUrl;
        await JS.InvokeVoidAsync("open", _generatedMapUrl, "_blank");
    }

    private static Event ToEvent(EventDto dto)
    {
        int year  = 2000 + dto.Idat / 10000;
        int month = dto.Idat / 100 % 100;
        int day   = dto.Idat % 100;
        int hour  = dto.Itim / 10000;
        int min   = dto.Itim / 100 % 100;
        int sec   = dto.Itim % 100;

        return new Event
        {
            Id   = dto.N,
            Dt   = new DateTime(year, month, day, hour, min, sec),
            N    = dto.N,
            X    = dto.X ?? 0,
            Y    = dto.Y ?? 0,
            Z    = dto.Z ?? 0,
            E    = dto.E ?? 0,
            Magn = dto.Magn ?? 0,
            Proc = dto.Proc ?? 0,
            Ampl = dto.Ampl ?? 0,
            NpActual = dto.NpActual,
            RqMin    = dto.RqMin,
            RqMax    = dto.RqMax,
            GpActual = dto.GpActual,
            AmplMax  = dto.AmplMax,
            EMax     = dto.EMax,
        };
    }

    private async Task SaveMap()
    {
        if (_isSaveButtonDisabled) return;

        var dateFrom = _dateRange[0]!.Value.ToString("yyyy-MM-dd");
        var dateTo   = _dateRange[1]!.Value.ToString("yyyy-MM-dd");
        var fileName = $"{ObjectState.SelectedObjName}_{dateFrom}_{dateTo}_{(int)(_minEnergy ?? 0)}.html";

        await JS.InvokeVoidAsync("eval",
            $"var a=document.createElement('a');" +
            $"a.href='{_generatedMapUrl}';" +
            $"a.download='{fileName}';" +
            $"document.body.appendChild(a);a.click();document.body.removeChild(a);");
    }
}
