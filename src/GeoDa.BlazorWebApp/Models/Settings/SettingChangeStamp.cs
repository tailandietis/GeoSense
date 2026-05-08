using System;

namespace GeoDa.BlazorWebApp.Models.Settings;

public class SettingChangeStamp
{
    public long Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string SettingName { get; set; } = string.Empty;

    public string OldValue { get; set; } = string.Empty;

    public string NewValue { get; set; } = string.Empty;

    public DateTime Dt { get; set; }
}
