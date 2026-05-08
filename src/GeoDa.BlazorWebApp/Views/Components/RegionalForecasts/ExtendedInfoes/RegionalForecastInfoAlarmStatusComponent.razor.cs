using GeoDa.BlazorWebApp.Models.Gui.Colors;
using GeoDa.BlazorWebApp.Services.WebAppUtils;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Globalization;
using System.Security.AccessControl;

namespace GeoDa.BlazorWebApp.Views.Components.RegionalForecasts.ExtendedInfoes;

public partial class RegionalForecastInfoAlarmStatusComponent
{
    [Inject]
    private IWebAppUtils Utils { get; set; } = default!;

    [Parameter]
    public EnergyAssessment EnergyAssessment { get; set; } = default!;

    [Parameter]
    public GpnsCountInfo GpnsCountInfo { get; set; } = default!;

    [Parameter]
    public ObjectStatus ObjectStatus { get; set; }

    [Parameter]
    public List<Event> AlarmEvents { get; set; } = default!;

    // Private
    private string GetGpnsCountLabelLabel() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
           quality: GpnsCountInfo.Quality,
           prefix: "Количество геофонов (шт.): ",
           goodData: GpnsCountInfo.Value.ToString());

    private string GetAlarmLimitValueLabel() =>
       Utils.BuildLabel(objectStatus: ObjectStatus,
           quality: EnergyAssessment.Report.Quality,
           prefix: "Порог (Дж): ",
           goodData: EnergyAssessment.Object.EnergyLimit.ToString("0.00", CultureInfo.InvariantCulture));

    private string GetAlarmCheckIntervalLabel() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: EnergyAssessment.Report.Quality,
            prefix: "Наблюдаемый временной интервал (дней): ",
            goodData: EnergyAssessment.Object.CheckIntervalInDays.ToString());

    private string GetDateTimeAlarmCheckLabel() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: EnergyAssessment.Report.Quality,
            prefix: "Дата/время получения данных: ",
            goodData: $"{EnergyAssessment.Object.DateTimeOfAssessment:G}");

    private string GetAlarmStatusLabel() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: EnergyAssessment.Report.Quality,
            prefix: "",
            goodData: EnergyAssessment.Report.IsAlarm
                ? "ОПАСНО (есть события за заданный период, превышающие порог)"
                : "Нет событий за заданный период, превышающих порог",
            badData: "Некорректные данные");

    private string GetAlarmEValueLabel() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: EnergyAssessment.Report.Quality,
            prefix: "Энергия события (Дж): ",
            goodData: EnergyAssessment.Report.Energy.ToString("0.00", CultureInfo.InvariantCulture));

    private string GetAlarmDateTimeStamp() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: EnergyAssessment.Report.Quality,
            prefix: "Временная метка события: ",
            goodData: $"{EnergyAssessment.Report.DateTimeOfItem:G}");

    private GeoDaColor GetAlarmStateBgTextColor() =>
        ObjectStatus switch
        {
            ObjectStatus.Ok => EnergyAssessment.Report.Quality switch
            {
                Quality.Good => EnergyAssessment.Report.IsAlarm switch
                {
                    true => GeoDaColor.Red,
                    false => GeoDaColor.Green,
                },
                Quality.Bad => GeoDaColor.Red,
                _ => GeoDaColor.Blue
            },
            ObjectStatus.DbError => GeoDaColor.Yellow,
            ObjectStatus.Absent => GeoDaColor.Gray,
            ObjectStatus.HasDuplicate => GeoDaColor.Gray,
            ObjectStatus.NotConnected => GeoDaColor.Red,
            ObjectStatus.Uncertain => GeoDaColor.Blue,
            _ => GeoDaColor.Blue
        };

    private GeoDaColor GetAlarmStateFgTextColor() =>
       ObjectStatus switch
       {
           ObjectStatus.Ok => EnergyAssessment.Report.Quality switch
           {
               Quality.Good => EnergyAssessment.Report.IsAlarm switch
               {
                   true => GeoDaColor.White,
                   false => GeoDaColor.White,
               },
               Quality.Bad => GeoDaColor.White,
               _ => GeoDaColor.White
           },
           ObjectStatus.DbError => GeoDaColor.Black,
           ObjectStatus.Absent => GeoDaColor.White,
           ObjectStatus.HasDuplicate => GeoDaColor.White,
           ObjectStatus.NotConnected => GeoDaColor.White,
           ObjectStatus.Uncertain => GeoDaColor.White,
           _ => GeoDaColor.White
       };

    // IDs
    private string GetCardId() =>
        $"rf_card";

    private string GetGpnsCountLabelId() =>
        "rf_gpns_count_label";

    private string GetAlarmLimitValueLabelId() =>
        "rf_alarm_limit_value_label";

    private string GetAlarmCheckIntervalLabelId() =>
        "rf_alarm_check_interval_label";

    private string GetDateTimeAlarmCheckLabelId() =>
        "rf_datetime_alarm_check_label";
}
