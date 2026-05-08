using AntDesign;
using GeoDa.Application.RegionalForecasts.Services;
using GeoDa.Infrastructure.Services.DateTimes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using GeoDa.Domain.Models;
using System.Linq;
using GeoDa.Domain.GeneralForecasts.Models;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class TimeWorkPage
{
    [Inject]
    private IMessageService MessageService { get; set; } = default!;

    [Inject]
    private ILogger<TimeWorkPage> Logger { get; set; } = default!;

    [Inject]
    private IRegionalForecastService RfService { get; set; } = default!;

    [Inject]
    private IDateTimeService DateTimeService { get; set; } = default!;

    [Parameter]
    public string ObjectName { get; set; } = string.Empty;

    private DateTime? _dateStart;
    private DateTime? _dateEnd;

    private List<CascaderNode> _objectNames = new();

    private bool _isGetDataButtonDisable = true;

    private string _workTime = string.Empty;

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

    private void OnDateStartChanged(DateTime? dateValue)
    {
        if (dateValue == null)
        {
            MessageService.Error("Значение даты \"До\" равно null");
            return;
        }

        _dateStart = dateValue;

        UpdateButtonStatus();
    }

    private void OnDateEndChanged(DateTime? dateValue)
    {
        if (dateValue == null)
        {
            MessageService.Error("Значение даты \"До\" равно null");
            UpdateButtonStatus();
            return;
        }

        _dateEnd = dateValue.Value.AddHours(23)
            .AddMinutes(59)
            .AddSeconds(59)
            .AddSeconds(11);

        if (_dateEnd < _dateStart)
            MessageService.Error("Значение даты \"До\" должно быть больше, чем \"От\"");

        UpdateButtonStatus();
    }
        
    // Private
    private void UpdateButtonStatus()
    {
        if (_dateStart != null && _dateEnd != null
            && (string.IsNullOrEmpty(ObjectName) is false))
        {
            if (_dateStart <= _dateEnd)
            {
                _isGetDataButtonDisable = false;
                return;
            }
        }

        _isGetDataButtonDisable = true;
    }

    private void ResetData()
    {
        _workTime = string.Empty;
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

    private void CalcWorkTime()
    {
        if (_dateStart == null || _dateEnd == null)
        {
            MessageService.Error("Необходимо задать дату и время");
            return;
        }
        
        (var objectStatus, var workTime) = RfService.GetWorkTime(ObjectName, _dateStart.Value, _dateEnd.Value);

        if(objectStatus != ObjectStatus.Ok)
        {
            MessageService.Error("Ошибка при получении данных о работе объекта");
            return;
        }

        var workTimeResult = string.Empty;

        if (workTime.Days > 0)
            workTimeResult += $"{workTime.Days} д. ";


        workTimeResult += $" {workTime.Hours} ч. {workTime.Minutes} мин.";

        _workTime = workTimeResult;
    }

    private string GetTitle() =>
        "Расчет времени работы системы";
}
