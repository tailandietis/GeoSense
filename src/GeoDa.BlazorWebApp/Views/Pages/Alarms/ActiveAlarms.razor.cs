using GeoDa.Application.AlarmMonitors.Services.AlarmMonitorServices;
using GeoDa.BlazorWebApp.Models.Alarms;
using GeoDa.BlazorWebApp.Services.Observers;
using GeoDa.Domain.AlarmMonitors.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace GeoDa.BlazorWebApp.Views.Pages.Alarms;

public partial class ActiveAlarms : IObserverClient, IDisposable
{
    [Inject]
    private IObserverService ObserverService { get; set; } = default!;

    [Inject]
    private IAlarmMonitorService AlarmMonitorService { get; set; } = default!;

    private readonly List<ActiveAlarm> _alarms = new();

    // Override
    protected override void OnInitialized()
    {
        if (!ObserverService.IsRegistered(this))
            ObserverService.RegisterObserver(this);

        UpdateAlarmsTable();

        base.OnInitialized();
    }

    // IObserverClient
    public void Update(object data)
    {
        UpdateAlarmsTable();

        InvokeAsync(() => StateHasChanged());
    }

    // IDisposable
    public void Dispose() =>
        ObserverService.RemoveObserver(this);

    // Private
    private void UpdateAlarmsTable()
    {
        var activeAlarms = AlarmMonitorService.GetAllActiveAlarms();

        _alarms.Clear();

        foreach (var alarm in activeAlarms)
        {
            var activeAlarm = new ActiveAlarm
            {
                Msg = AlarmMonitorService.GetTextDescription(alarm.AlarmId),
                DateTime = alarm.DateTime,
                AlarmStatus = alarm.AlarmStatus,
            };

            _alarms.Add(activeAlarm);
        }
    }

    private string GetAlarmStatusDescription(AlarmStatus alarmStatus) =>
        alarmStatus switch
        {
            AlarmStatus.Uncertain => "Неопределено",

            AlarmStatus.Active => "Активен",
            AlarmStatus.Deactived => "Отсутствует",
            AlarmStatus.Confirmed => "Подтвержден",
            _ => "Неопределено"
        };


    private string GetTitle() =>
        "Журнал активных алармов";
}
