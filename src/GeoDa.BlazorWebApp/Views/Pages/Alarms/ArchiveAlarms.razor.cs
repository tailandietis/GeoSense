using AntDesign;
using GeoDa.Application.AlarmMonitors.Models;
using GeoDa.Application.AlarmMonitors.Services.AlarmMonitorServices;
using GeoDa.Application.RegionalForecasts.Services;
using GeoDa.BlazorWebApp.Models.Alarms;
using GeoDa.Domain.AlarmMonitors.Models;
using GeoDa.Domain.AlarmMonitors.Services;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.BlazorWebApp.Views.Pages.Alarms;

public partial class ArchiveAlarms
{
    [Inject]
    private ILogger<ArchiveAlarms> Logger { get; set; } = default!;

    [Inject]
    private IRegionalForecastService RfService { get; set; } = default!;

    [Inject]
    private IAlarmCodeFactory AlarmCodeFactory { get; set; } = default!;

    [Inject]
    private IAlarmIdFactory AlarmIdFactory { get; set; } = default!;

    

    [Inject]
    private IAlarmMonitorService AlarmMonitorService { get; set; } = default!;

    private string _objectType = string.Empty;
    private string _objectName = string.Empty;

    private DateTime? _startDt;
    private DateTime? _endDt;

    private readonly List<CascaderNode> _objectTypes = new();
    private readonly List<CascaderNode> _objectNames = new();

    private List<ActiveAlarm> _archAlarms = new();

    private bool _isGetDataButtonDisable = true;

    // Override
    protected override void OnParametersSet()
    {
        _objectTypes.Add(new CascaderNode() { Value = "rf", Label = "Региональный прогноз" });
        _objectTypes.Add(new CascaderNode() { Value = "cf", Label = "Текущий прогноз" });

        if (_objectType != string.Empty)
            UpdateObjectNames(_objectType);

        base.OnParametersSet();
    }

    // Private
    private void OnAlarmSourceChanged(CascaderNode[] selectedNodes)
    {
        if (selectedNodes.Length == 1)
        {
            _objectType = selectedNodes.First().Value;

            UpdateObjectNames(selectedNodes.First().Value);
        }
    }

    private void OnObjectNameChanged(CascaderNode[] selectedNodes)
    {
        if (selectedNodes.Length == 1)
            _objectName = selectedNodes.First().Value;
    }

    private void OnDateChange(DateRangeChangedEventArgs args)
    {
        if (args.Dates is not null && args.Dates.Length == 2)
        {
            _startDt = args.Dates[0];
            _endDt = args.Dates[1];

            UpdateGetDataFromDbButtonStatus();
        }
    }

    private void UpdateObjectNames(string objectType)
    {
        (var status, var data) = GetObjectsDataAndStatuses(objectType);

        if (status == OpStatus.Ok)
        {
            _objectNames.Clear();

            _objectNames.AddRange(data.Select(v => new CascaderNode() { Value = v.Key, Label = v.Key }));
        }
    }

    private (OpStatus, Dictionary<string, ObjectInfoAndStatus>) GetObjectsDataAndStatuses(string objectType)
    {
        try
        {
            var result = objectType switch
            {
                "rf" => (OpStatus.Ok, RfService.GetObjectInfoAndStatuses()),
                _ => (OpStatus.GetDataError, new())
            };

            return result;            
        }
        catch (Exception ex)
        {
            Logger.LogError(exception: ex, message: ex.Message);

            return (OpStatus.GetDataError, new());
        }
    }

    private void GetDataFromDb()
    {
        var moduleFamilyCode = GetModuleFamilyCode(_objectType);
        var objId = GetObjectId(_objectType, _objectName);

        if (moduleFamilyCode > 0 && objId > 0 && _startDt != null && _endDt != null)
            UpdateTable(moduleFamilyCode, objId, (DateTime)_startDt, (DateTime)_endDt);
    }

    private void UpdateTable(int moduleFamilyName, int objId, DateTime startDt, DateTime endDt)
    {
        try
        {
            var data = AlarmMonitorService.GetAlarmsFromArchive(moduleFamilyName, objId, startDt, endDt)
                .OrderByDescending(x => x.DateTime)
                .ToList();

            FillActiveAlarms(data);
        }
        catch (Exception ex)
        {
            Logger.LogError(exception: ex, message: ex.Message);
        }
    }

    private void FillActiveAlarms(IReadOnlyList<AlarmInfo> data)
    {
        _archAlarms.Clear();

        foreach (var alarm in data)
        {
            var archAlarmItem = new ActiveAlarm
            {
                Msg = AlarmMonitorService.GetTextDescription(
                    AlarmIdFactory.Create(
                        objectId: alarm.AlarmId.ObjectId,
                        alarmCode: AlarmCodeFactory.Create(
                            moduleFamilyCode: alarm.AlarmId.AlarmCode.ModuleFamilyCode,
                            serviceFamilyCode: alarm.AlarmId.AlarmCode.ServiceFamilyCode,
                            errorCode: alarm.AlarmId.AlarmCode.ErrorCode))),
                DateTime = alarm.DateTime,
                AlarmStatus = alarm.AlarmStatus
            };

            _archAlarms.Add(archAlarmItem);
        }
    }

    private int GetModuleFamilyCode(string objectType) =>
        objectType switch
        {
            "rf" => 1,
            "cf" => 2,
            _ => -1
        };

    private int GetObjectId(string objectType, string objectName)
    {
        int objId = -1;
        (var status, var data) = GetObjectsDataAndStatuses(_objectType);

        if (status == OpStatus.Ok && data.ContainsKey(_objectName))
            objId = data[_objectName].ObjectInfo.Id;

        return objId;
    }

    private void UpdateGetDataFromDbButtonStatus()
    {
        if (_startDt != null && _endDt != null)
            _isGetDataButtonDisable = false;
        else
            _isGetDataButtonDisable = true;
    }

    private string GetAlarmStatusDescription(AlarmStatus alarmStatus) =>
        alarmStatus switch
        {
            AlarmStatus.Uncertain => "Неопределено",

            AlarmStatus.Active => "Активен",
            AlarmStatus.Deactived => "Не активен",
            AlarmStatus.Confirmed => "Подтвержден",
            _ => "Неопределено"
        };

    private string GetTitle() => "Архив алармов";
}
