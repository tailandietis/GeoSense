using GeoDa.Application.GeneralForecasts.Services.Utils;
using GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies;
using GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies.Dtos;
using GeoDa.BlazorWebApp.Services.MLBuilder;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class MlVsGeoDaPage : ComponentBase
{
    [Inject] private IMLBuilderService MLBuilderService { get; set; } = default!;
    [Inject] private IGsParamFEnergyRepository GsParamFRepo { get; set; } = default!;
    [Inject] private MLObjectStateService ObjectState { get; set; } = default!;
    [Inject] private IGeneralForecastUtilsService Utils { get; set; } = default!;

    private bool _hasData = false;
    private bool _isLoading = false;
    private string _errorMessage = "";
    private DateTime?[] _dateRange = new DateTime?[] { DateTime.Now.AddDays(-30), DateTime.Now };
    private string _filterMode = "all";

    private int _totalEvents = 0;
    private int _matchCount = 0;
    private int _mlHigherCount = 0;
    private int _geodaHigherCount = 0;
    private double _matchRate => _totalEvents > 0 ? (double)_matchCount / _totalEvents : 0;

    private List<ComparisonRow> _allRows = new();
    private List<ComparisonRow> _filteredRows = new();

    protected override void OnInitialized()
    {
        ObjectState.GetObjects();
    }

    private async Task LoadData()
    {
        if (ObjectState.SelectedObjId is null)
        {
            _errorMessage = "Выберите объект в ML Dashboard";
            return;
        }

        _isLoading = true;
        _errorMessage = "";
        StateHasChanged();

        try
        {
            if (!MLBuilderService.IsTrainResultReady())
            {
                _errorMessage = "Модель не обучена. Перейдите в ML Dashboard и нажмите Обучить.";
                _hasData = false;
                return;
            }

            var start = _dateRange[0] ?? DateTime.Now.AddDays(-30);
            var end   = _dateRange[1] ?? DateTime.Now;
            var objId = ObjectState.SelectedObjId.Value;

            var predictResult = await MLBuilderService.PredictAsync(objId, start, end);
            if (!predictResult.Success)
            {
                _errorMessage = predictResult.ErrorMessage ?? "Ошибка predict";
                _hasData = false;
                return;
            }

            GsParamFRepo.DbConnectionString = Utils.BuildDbConnectionString("DbPeleng");
            var fRecords = GsParamFRepo.SelectGsParamFEnergy(objId, start, end);
            var fStatusByDate = BuildFStatusByDate(fRecords);

            _allRows = BuildComparisonRows(predictResult, fStatusByDate);
            _hasData = _allRows.Count > 0;
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
            _hasData = false;
        }
        finally
        {
            ApplyFilter();
            UpdateStats();
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void OnFilterModeChanged(string value)
    {
        _filterMode = value;
        ApplyFilter();
        StateHasChanged();
    }

    private void OnDateRangeChanged(DateTime?[] value)
    {
        _dateRange = value;
        ApplyFilter();
        StateHasChanged();
    }

    private static Dictionary<string, int> BuildFStatusByDate(List<GsParamFEnergyDto> records)
    {
        return records
            .GroupBy(r => r.Dt.ToString("yyyy-MM-dd"))
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(r => r.ParamFStatus)
                      .OrderByDescending(x => x.Count())
                      .First().Key);
    }

    private static List<ComparisonRow> BuildComparisonRows(
        MLPredictResult predict,
        Dictionary<string, int> fStatusByDate)
    {
        var rows = new List<ComparisonRow>();

        foreach (var pred in predict.Events)
        {
            var dateStr = IdatToDateString(pred.Idat);
            var fStatus = fStatusByDate.TryGetValue(dateStr, out var fs) ? fs : 5;
            var (geoDaLevel, geoDaColor, geoDaClassIndex) = MapFStatus(fStatus);

            var e = pred.Energy;
            var energyStr = e >= 1_000_000
                ? $"{e / 1_000_000:F2}M"
                : e >= 1000
                    ? $"{e / 1000:F1}k"
                    : $"{e:F0}";

            rows.Add(new ComparisonRow(
                pred.DateTimeStr,
                energyStr,
                pred.X.ToString("F0"),
                pred.Y.ToString("F0"),
                pred.Z.ToString("F0"),
                geoDaLevel,
                geoDaColor,
                geoDaClassIndex,
                pred.LabelName,
                pred.AntdColor,
                pred.Label));
        }
        return rows;
    }

    private static string IdatToDateString(long idat)
    {
        int year  = 2000 + (int)(idat / 10000);
        int month = (int)(idat / 100 % 100);
        int day   = (int)(idat % 100);
        return $"{year}-{month:D2}-{day:D2}";
    }

    private static (string level, string color, int classIdx) MapFStatus(int fStatus) => fStatus switch
    {
        1 => ("Фон",       "default", 0),
        2 => ("Уровень 1", "success", 1),
        3 => ("Уровень 2", "warning", 2),
        4 => ("Уровень 3", "orange",  3),
        _ => ("Уровень 4", "error",   3),
    };

    private void ApplyFilter()
    {
        var rows = _allRows.AsEnumerable();

        if (_dateRange[0].HasValue)
            rows = rows.Where(r => ParseRowDate(r.DateTime) >= _dateRange[0]!.Value.Date);
        if (_dateRange[1].HasValue)
            rows = rows.Where(r => ParseRowDate(r.DateTime) <= _dateRange[1]!.Value.Date);

        _filteredRows = _filterMode switch
        {
            "mismatch"     => rows.Where(r => !r.IsMatch).ToList(),
            "ml_higher"    => rows.Where(r => r.MlClassIndex > r.GeoDaClassIndex).ToList(),
            "geoda_higher" => rows.Where(r => r.GeoDaClassIndex > r.MlClassIndex).ToList(),
            _              => rows.ToList()
        };
    }

    private static DateTime ParseRowDate(string dt)
    {
        return DateTime.TryParseExact(dt, "dd.MM.yyyy HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
            ? result
            : DateTime.MinValue;
    }

    private void UpdateStats()
    {
        _totalEvents      = _allRows.Count;
        _matchCount       = _allRows.Count(r => r.IsMatch);
        _mlHigherCount    = _allRows.Count(r => r.MlClassIndex > r.GeoDaClassIndex);
        _geodaHigherCount = _allRows.Count(r => r.GeoDaClassIndex > r.MlClassIndex);
    }
}

public record ComparisonRow(
    string DateTime,
    string Energy,
    string X,
    string Y,
    string Z,
    string GeoDaLevel,
    string GeoDaColor,
    int GeoDaClassIndex,
    string MlLevel,
    string MlColor,
    int MlClassIndex)
{
    public bool IsMatch => GeoDaClassIndex == MlClassIndex;
}
