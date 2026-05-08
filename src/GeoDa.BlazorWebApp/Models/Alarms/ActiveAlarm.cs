using GeoDa.Domain.AlarmMonitors.Models;
using System;

namespace GeoDa.BlazorWebApp.Models.Alarms;

public class ActiveAlarm
{
    public string Msg { get; set; } = string.Empty;

    public DateTime DateTime { get; set; }

    public AlarmStatus AlarmStatus { get; set; }
}
