using GeoDa.BlazorWebApp.Services.WebAppUtils;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace GeoDa.BlazorWebApp.Views.Components.RegionalForecasts.ExtendedInfoes;

public partial class RegionalForecastInfoStatCalcComponent
{
    [Inject]
    private IWebAppUtils Utils { get; set; } = default!;

    [Parameter]
    public ObjectStatistics Statistics { get; set; } = default!;

    [Parameter]
    public ObjectStatus ObjectStatus { get; set; }

    // Private
    private string GetDateTimeStatCalcLabel() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: Statistics.Quality,
            prefix: "Дата формирования отчета: ",
            goodData: $"{Statistics.DateTimeOfStatCalc:G}");

    private string GetStatCalcIntervalLabel() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: Statistics.Quality,
            prefix: "Расчетный период (дней): ",
            goodData: Statistics.StatCalcInterval.ToString());

    private string GetMinEValueLabel() =>
       Utils.BuildLabel(objectStatus: ObjectStatus,
           quality: Statistics.Quality,
           prefix: "Минимальное значение в наборе данных (Дж): ",
           goodData: Statistics.Statistics.MinVal.ToString("0.00", CultureInfo.InvariantCulture));
    private string GetMaxEValueLabel() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: Statistics.Quality,
            prefix: "Максимальное значение в наборе данных (Дж): ",
            goodData: Statistics.Statistics.MaxVal.ToString("0.00", CultureInfo.InvariantCulture));
    private string GetQ70Label() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: Statistics.Quality,
            prefix: "70.0% событий имеют значения энергии меньше, чем (Дж): ",
            goodData: Statistics.Statistics.Q70.ToString("0.00", CultureInfo.InvariantCulture));

    private string GetQ80Label() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: Statistics.Quality,
            prefix: "80.0% событий имеют значения энергии меньше, чем (Дж): ",
            goodData: Statistics.Statistics.Q80.ToString("0.00", CultureInfo.InvariantCulture));

    private string GetQ90Label() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: Statistics.Quality,
            prefix: "90.0% событий имеют значения энергии меньше, чем (Дж): ",
            goodData: Statistics.Statistics.Q90.ToString("0.00", CultureInfo.InvariantCulture));

    private string GetQ95Label() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: Statistics.Quality,
            prefix: "95.0% событий имеют значения энергии меньше, чем (Дж): ",
            goodData: Statistics.Statistics.Q95.ToString("0.00", CultureInfo.InvariantCulture));

    private string GetQ99Label() =>
        Utils.BuildLabel(objectStatus: ObjectStatus,
            quality: Statistics.Quality,
            prefix: "99.0% событий имеют значения энергии меньше, чем (Дж): ",
            goodData: Statistics.Statistics.Q99.ToString("0.00", CultureInfo.InvariantCulture));

    private string GetStatCalcIntervalLabelId() => "rf_stat_calc_interval_label";
    private string GetDateTimeStatCalcLabelId() => "rf_date_time_stat_calc_label";
    private string GetMinEValueLabelId() => "rf_min_e_value_label";
    private string GetMaxEValueLabelId() => "rf_max_e_value_label";
    private string GetQ70LabelId() => "rf_q70_label";
    private string GetQ80LabelId() => "rf_q80_label";
    private string GetQ90LabelId() => "rf_q90_label";
    private string GetQ95LabelId() => "rf_q95_label";
    private string GetQ99LabelId() => "rf_q99_label";
}
