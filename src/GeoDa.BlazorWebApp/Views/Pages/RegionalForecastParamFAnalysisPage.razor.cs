using AntDesign;
using GeoDa.Application.RegionalForecasts.Services;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class RegionalForecastParamFAnalysisPage
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IRegionalForecastService RfService { get; set; } = default!;

    [Inject]
    private IMessageService MessageService { get; set; } = default!;

    [Inject]
    private ILogger<RegionalForecastParamFAnalysisPage> Logger { get; set; } = default!;

    [Parameter]
    public string ObjectName { get; set; } = string.Empty;

    private DateTime? _dateCalcF;

    private List<CascaderNode> _objectNames = new();

    private bool _isCalcFButtonDisable = true;

    private double _blockSize = 0;
    private int _timeSpanMinImpact = 0;
    private int _timeSpanReg = 0;
    private double _bgEnergy = 0;

    // Override
    protected override void OnParametersSet()
    {
        UpdateObjectNames();

        base.OnParametersSet();
    }

    // Events
    private void OnObjectNameChanged(CascaderNode[] selectedNodes)
    {
        if (selectedNodes.Length == 1)
        {
            ResetData();
            ObjectName = selectedNodes.First().Value;
        }

        UpdateButtonStatus();
    }

    private void OnDateCalcFChanged(DateTime? dateValue)
    {
        if (dateValue == null)
        {
            MessageService.Error("Значение даты равно null");
            return;
        }

        _dateCalcF = dateValue;

        UpdateButtonStatus();
    }

    // Private
    private void UpdateButtonStatus()
    {
        if (_dateCalcF == null || (string.IsNullOrEmpty(ObjectName) is true))
        {
            _isCalcFButtonDisable = true;
            return;
        }

        _isCalcFButtonDisable = false;
    }

    private void ResetData()
    {        
    }

    private void UpdateObjectNames()
    {
        (var status, var data) = GetObjectsDataAndStatuses();
        if (status == OpStatus.Ok)
        {
            _objectNames.Clear();
            _objectNames.AddRange(data.Select(v => new CascaderNode() { Value = v.Key, Label = v.Key }));
        }
    }

    private (OpStatus, Dictionary<string, ObjectInfoAndStatus>) GetObjectsDataAndStatuses()
    {
        try
        {
            var result = (OpStatus.Ok, RfService.GetObjectInfoAndStatuses());

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(exception: ex, message: ex.Message);

            return (OpStatus.GetDataError, new());
        }
    }

    private string GetTitle() =>
        "Расчет параметра F и оценка энергий событий";

    private async Task<bool> IsAllParamsOk()
    {
        if (_dateCalcF is null)
        {
            await MessageService.Error("Необходимо задать дату расчета параметра F");
            return false;
        }

        if (_blockSize <= 0)
        {
            await MessageService.Error("Размер блока должен быть больше нуля");
            return false;
        }

        if (_timeSpanMinImpact <= 0)
        {
            await MessageService.Error("Время влияния события должно быть больше нуля");
            return false;
        }

        if (_timeSpanReg <= 0)
        {
            await MessageService.Error("Время регистрации должно быть больше нуля");
            return false;
        }

        if (_bgEnergy <= 0)
        {
            await MessageService.Error("Значение фоновой энергии должно быть больше нуля");
            return false;
        }

        if (_timeSpanReg > _timeSpanMinImpact)
        {
            await MessageService.Error("Время регистрации должно быть меньше времени влияния события");
            return false;
        }

        return true;
    }

    private async Task CalcParamFTime()
    {
        if ((await IsAllParamsOk()) is false)
            return;

        var dataFolder = "data";
        var archFolder = "arch";
        var pathForFileStore = Path.Combine("wwwroot", dataFolder, archFolder);

        (var status, var blocksInfo) = await RfService.GetRfBlocksInfoAsync(ObjectName, _dateCalcF.Value, _blockSize, 
            _timeSpanReg, _timeSpanMinImpact, _bgEnergy);

        if(status != ObjectStatus.Ok)
        {
            await MessageService.Error($"Нет данных для объекта {ObjectName}");
            return;
        }

        (var isOk, string fileName) = RfService.PreparedFileWithArchBlocksInfo(ObjectName, pathForFileStore, blocksInfo);

        if (isOk)
        {
            await DownloadFileFromURL(fileName, $"/{dataFolder}/{archFolder}/{fileName}");            
        }
        else
        {
            ResetData();
        }
    }

    private async Task DownloadFileFromURL(string fileName, string fileUrl)
    {
        await JSRuntime.InvokeVoidAsync("triggerFileDownload", fileName, fileUrl);
    }
}
