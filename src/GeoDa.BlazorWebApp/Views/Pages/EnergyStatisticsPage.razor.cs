using AntDesign;
using GeoDa.Application.RegionalForecasts.Services;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class EnergyStatisticsPage : ComponentBase
{
    [Inject] private IMessageService MessageService { get; set; } = default!;
    [Inject] private ILogger<EnergyStatisticsPage> Logger { get; set; } = default!;
    [Inject] private IRegionalForecastService RfService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter] public string ObjectName { get; set; } = string.Empty;

    private DateTime? _dateStart;
    private DateTime? _dateEnd;
    private List<CascaderNode> _objectNames = new();
    private bool _isCalculateButtonDisabled = true;
    private string _resultText = string.Empty;

    protected override void OnParametersSet()
    {
        UpdateObjectNames();
        base.OnParametersSet();
    }

    private void OnObjectNameChanged(CascaderNode[] selectedNodes)
    {
        if (selectedNodes.Length == 1)
        {
            ResetData();
            ObjectName = selectedNodes.First().Value;
        }
        UpdateButtonStatus();
    }

    private void OnDateStartChanged(DateTime? dateValue)
    {
        if (dateValue == null)
        {
            MessageService.Error("Значение даты \"От\" не задано");
            return;
        }
        _dateStart = dateValue;
        UpdateButtonStatus();
    }

    private void OnDateEndChanged(DateTime? dateValue)
    {
        if (dateValue == null)
        {
            MessageService.Error("Значение даты \"До\" не задано");
            UpdateButtonStatus();
            return;
        }

        _dateEnd = dateValue.Value.Date.AddDays(1).AddSeconds(-1);

        if (_dateEnd < _dateStart)
            MessageService.Error("Дата \"До\" должна быть не меньше даты \"От\"");

        UpdateButtonStatus();
    }

    private void UpdateButtonStatus()
    {
        _isCalculateButtonDisabled = string.IsNullOrEmpty(ObjectName) ||
                                    _dateStart == null ||
                                    _dateEnd == null ||
                                    _dateStart > _dateEnd;
    }

    private void ResetData()
    {
        _resultText = string.Empty;
    }

    private void UpdateObjectNames()
    {
        var (status, data) = GetObjectsDataAndStatuses();
        if (status == OpStatus.Ok)
        {
            _objectNames = data.Select(v => new CascaderNode { Value = v.Key, Label = v.Key }).ToList();
        }
    }

    private (OpStatus, Dictionary<string, ObjectInfoAndStatus>) GetObjectsDataAndStatuses()
    {
        try
        {
            return (OpStatus.Ok, RfService.GetObjectInfoAndStatuses());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка загрузки списка объектов");
            return (OpStatus.GetDataError, new Dictionary<string, ObjectInfoAndStatus>());
        }
    }

    private void CalculateEnergyStatistics()
    {
        if (_dateStart == null || _dateEnd == null)
        {
            MessageService.Error("Укажите обе даты");
            return;
        }

        var (status, stats) = LoadStatistics();

        if (status != ObjectStatus.Ok)
        {
            MessageService.Error($"Не удалось загрузить статистику для объекта {ObjectName}");
            ResetData();
            return;
        }

        var dateStart = _dateStart.Value.Date;
        var dateEnd   = _dateEnd.Value.Date;

        if (stats.Statistics == null || stats.Quality == Quality.Bad)
        {
            _resultText = $"Период: от {dateStart:yyyy-MM-dd} до {dateEnd:yyyy-MM-dd}. Нет данных.";
            return;
        }

        var s = stats.Statistics;
        _resultText = $"Период: от {dateStart:yyyy-MM-dd} до {dateEnd:yyyy-MM-dd}\n" +
              $"Событий: {s.NRows}\n" +
              $"Мин. энергия: {s.MinVal:F2} Дж\n" +
              $"Макс. энергия: {s.MaxVal:F2} Дж\n" +
              $"70-й процентиль: {s.Q70:F2} Дж\n" +
              $"80-й процентиль: {s.Q80:F2} Дж\n" +
              $"90-й процентиль: {s.Q90:F2} Дж\n" +
              $"95-й процентиль: {s.Q95:F2} Дж\n" +
              $"99-й процентиль: {s.Q99:F2} Дж\n" +
              $"Первое событие: {s.DateTimeOfOldestEvent:yyyy-MM-dd HH:mm:ss}\n" +
              $"Последнее событие: {s.DateTimeOfNewestEvent:yyyy-MM-dd HH:mm:ss}";
    }

    private async Task DownloadEnergyStatisticsCsv()
    {
        if (_dateStart == null || _dateEnd == null)
            return;

        var (status, stats) = LoadStatistics();

        if (status != ObjectStatus.Ok || stats.Statistics == null)
        {
            await MessageService.Error("Нет данных для выгрузки");
            return;
        }

        var dateStart = _dateStart.Value.Date;
        var dateEnd   = _dateEnd.Value.Date;
        var s = stats.Statistics;

        var csvLines = new List<string>
        {
            "Параметр;Значение",
            $"Период от;{dateStart:yyyy-MM-dd}",
            $"Период до;{dateEnd:yyyy-MM-dd}",
            $"Событий;{s.NRows}",
            $"Мин. энергия;{s.MinVal:F2}",
            $"Макс. энергия;{s.MaxVal:F2}",
            $"70-й процентиль;{s.Q70:F2}",
            $"80-й процентиль;{s.Q80:F2}",
            $"90-й процентиль;{s.Q90:F2}",
            $"95-й процентиль;{s.Q95:F2}",
            $"99-й процентиль;{s.Q99:F2}",
            $"Дата первого события;{s.DateTimeOfOldestEvent:yyyy-MM-dd HH:mm:ss}",
            $"Дата последнего события;{s.DateTimeOfNewestEvent:yyyy-MM-dd HH:mm:ss}"
        };

        var csvContent = string.Join("\n", csvLines);
        var fileName = $"energy_stats_{ObjectName}_{dateStart:ddMMyy}-{dateEnd:ddMMyy}.csv";

        var folder = Path.Combine("wwwroot", "data");
        Directory.CreateDirectory(folder);
        var fullPath = Path.Combine(folder, fileName);
        await File.WriteAllTextAsync(fullPath, csvContent, Encoding.UTF8);

        await JSRuntime.InvokeVoidAsync("triggerFileDownload", fileName, $"/data/{fileName}");
    }

    private (ObjectStatus, ObjectStatistics) LoadStatistics()
    {
        var dateStart    = _dateStart!.Value.Date;
        var dateEnd      = _dateEnd!.Value.Date;
        var intervalDays = (int)(dateEnd - dateStart).TotalDays;
        return RfService.GetObjectStatistics(ObjectName, dateEnd, intervalDays);
    }

    private string GetTitle() => "Статистика по энергии";
}