using AntDesign;
using GeoDa.Application.RegionalForecasts.Repository.RegionalForecastActualDataStores;
using GeoDa.Application.RegionalForecasts.Services;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using GeoDa.Infrastructure.Services.DateTimes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class ArchivePage
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IMessageService MessageService { get; set; } = default!;

    [Inject]
    private ILogger<ArchivePage> Logger { get; set; } = default!;

    [Inject]
    private IRegionalForecastService RfService { get; set; } = default!;

    [Inject]
    private IDateTimeService DateTimeService { get; set; }

    [Parameter]
    public string ObjectType { get; set; } = string.Empty;

    [Parameter]
    public string ObjectName { get; set; } = string.Empty;

    public string ArchiveType { get; set; } = string.Empty;

    private List<CascaderNode> _objectTypes = new();
    private List<CascaderNode> _objectNames = new();
    private List<CascaderNode> _archiveTypes = new();

    private DateTime? _dateStart;
    private DateTime? _dateEnd;
    private DateTime? _time;

    private double _minEnergy;

    private bool _isGetDataButtonDisable = true;
    private bool _isDataRequested = false;
    private bool _dataIsOk = false;

    protected override void OnParametersSet()
    {
        _objectTypes = new()
        {
            new CascaderNode() { Value = "rf", Label = "Региональный прогноз" },
        };

        if (ObjectType != string.Empty)
            UpdateObjectNames(ObjectType);

        _archiveTypes = new()
        {
            new CascaderNode() { Value = "events", Label = "Сейсмические события" },
            new CascaderNode() { Value = "gpns", Label = "Работа системы" },
            new CascaderNode() { Value = "paramf", Label = "Параметр F и энергия" }
        };

        base.OnParametersSet();
    }

    private void ResetData() { _dataIsOk = false; }

    private void OnForecastChanged(CascaderNode[] selectedNodes)
    {
        if (selectedNodes.Length == 1)
        {
            ObjectType = selectedNodes.First().Value;
            ResetData();
            UpdateObjectNames(selectedNodes.First().Value);
            _isDataRequested = false;
        }
    }

    private void OnObjectNameChanged(CascaderNode[] selectedNodes)
    {
        if (selectedNodes.Length == 1)
        {
            ResetData();
            ObjectName = selectedNodes.First().Value;
            _isDataRequested = false;
        }
    }

    private void OnArchiveTypeChanged(CascaderNode[] selectedNodes) { }

    private void OnDateStartChanged(DateTime? dateTime)
    {
        _dateStart = dateTime;
        _isDataRequested = false;
        UpdateButtonStatus();
    }

    private void OnDateEndChanged(DateTime? dateTime)
    {
        _dateEnd = dateTime;
        _isDataRequested = false;
        if (_dateStart > _dateEnd)
            MessageService.Error("Значение даты \"До\" должно быть больше, чем \"От\"");
        UpdateButtonStatus();
    }

    private void OnTimeChanged(DateTime? dateTime)
    {
        _time = dateTime;
        _isDataRequested = false;
        UpdateButtonStatus();
    }

    private void OnDataSourceChanged(int dataSource) { _isDataRequested = false; }

    private async Task DownloadFileFromURL(string fileName, string fileUrl)
    {
        await JSRuntime.InvokeVoidAsync("triggerFileDownload", fileName, fileUrl);
    }

    private async Task GetDataAndBuildReport()
    {
        _isDataRequested = true;

        if (ObjectType == "rf" && _dateStart is not null && _dateEnd is not null)
        {
            try
            {
                var fileFolder = "data";
                var pathForFileStore = Path.Combine("wwwroot", "data");

                if (ArchiveType == "events")
                {
                    (var isOk, string fileName) = RfService.PreparedFileWithEvents(ObjectName, pathForFileStore,
                        _dateStart.Value, _dateEnd.Value, _minEnergy);
                    if (isOk) { await DownloadFileFromURL(fileName, $"/{fileFolder}/{fileName}"); _dataIsOk = true; }
                    else ResetData();
                }
                else if (ArchiveType == "gpns")
                {
                    (var isOk, string fileName) = RfService.PreparedFileWithGpnsCount(ObjectName, pathForFileStore,
                        _dateStart.Value, _dateEnd.Value.AddSeconds(86399));
                    if (isOk) { await DownloadFileFromURL(fileName, $"/{fileFolder}/{fileName}"); _dataIsOk = true; }
                    else ResetData();
                }
                else if (ArchiveType == "paramf")
                {
                    (var isOk, string fileName) = RfService.PreparedFileWithBlocksInfo(ObjectName, pathForFileStore);
                    if (isOk) { await DownloadFileFromURL(fileName, $"/{fileFolder}/{fileName}"); _dataIsOk = true; }
                    else ResetData();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(exception: ex, message: ex.Message);
                ResetData();
            }
        }
    }

    private RenderFragment RenderObjectInfoBlock()
    {
        if (ObjectName != null && ObjectName != string.Empty && _isDataRequested)
        {
            if (_dataIsOk) return BuildBlankRenderFragment();
            return BuildRenderFragmentWithText("Ошибка при загрузке данных");
        }
        return BuildBlankRenderFragment();
    }

    private void UpdateButtonStatus()
    {
        if (_dateStart != null && _dateEnd != null && _dateStart <= _dateEnd && ObjectType == "rf")
            _isGetDataButtonDisable = false;
        else
            _isGetDataButtonDisable = true;
    }

    private void UpdateObjectNames(string objectType)
    {
        try
        {
            if (objectType == "rf")
            {
                var data = RfService.GetObjectInfoAndStatuses();
                _objectNames.Clear();
                _objectNames.AddRange(data.Select(v => new CascaderNode() { Value = v.Key, Label = v.Key }));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(exception: ex, message: ex.Message);
        }
    }

    private string GetTitle() => "Архив";
}
