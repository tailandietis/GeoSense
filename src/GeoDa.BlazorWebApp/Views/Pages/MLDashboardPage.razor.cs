using AntDesign;
using GeoDa.BlazorWebApp.Services.MLBuilder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class MLDashboardPage : ComponentBase
{
    [Inject] private IMLBuilderService MLBuilderService { get; set; } = default!;
    [Inject] private IMessageService MessageService { get; set; } = default!;
    [Inject] private ILogger<MLDashboardPage> Logger { get; set; } = default!;
    [Inject] private IOptions<MLBuilderSettings> MLSettings { get; set; } = default!;

    private bool _isTraining = false;
    private MLTrainResult? _trainResult;
    private string? _errorMessage;
    private Dictionary<string, string> _plotFiles = new();

    protected override void OnInitialized()
    {
        if (MLBuilderService.IsTrainResultReady())
        {
            _trainResult = MLBuilderService.GetLastTrainResult();
            LoadPlotFiles();
        }
    }

    private async Task TrainModel()
    {
        _isTraining = true;
        _errorMessage = null;
        _trainResult = null;
        StateHasChanged();

        _trainResult = await MLBuilderService.TrainAsync(string.Empty);

        if (_trainResult.Success)
        {
            LoadPlotFiles();
            await MessageService.Success("Модель успешно обучена");
        }
        else
        {
            _errorMessage = _trainResult.ErrorMessage;
            await MessageService.Error($"Ошибка: {_errorMessage}");
        }

        _isTraining = false;
        StateHasChanged();
    }

    private void LoadPlotFiles()
    {
        _plotFiles.Clear();
        var plotsDir = Path.Combine(MLSettings.Value.OutputDir, "plots");
        if (!Directory.Exists(plotsDir)) return;

        var mapping = new Dictionary<string, string>
        {
            ["xgb_confusion_matrix.png"] = "Матрица ошибок",
            ["xgb_feature_importance.png"] = "Важность признаков",
            ["baseline_comparison.png"] = "Сравнение с baseline",
            ["robustness.png"] = "Устойчивость к шуму",
            ["iso_anomaly_scores.png"] = "Anomaly Scores",
        };

        foreach (var (file, title) in mapping)
        {
            var fullPath = Path.Combine(plotsDir, file);
            if (File.Exists(fullPath))
                _plotFiles[title] = $"/ml-plots/{file}";
        }
    }

    private List<MetricRow> GetXgbMetrics()
    {
        if (_trainResult == null) return new();
        return new()
        {
            new("Accuracy", _trainResult.XgbAccuracy.ToString("P1")),
            new("F1-macro", _trainResult.XgbF1Macro.ToString("F3")),
            new("ROC-AUC", _trainResult.XgbRocAuc.ToString("F3")),
            new("CV F1-macro (5-fold)", _trainResult.XgbCvF1Mean.ToString("F3")),
        };
    }

    private List<CompareRow> GetBaselineComparison()
    {
        if (_trainResult == null) return new();
        var rows = new List<CompareRow>();

        var methodNames = new Dictionary<string, string>
        {
            ["energy_threshold"] = "Порог по энергии (GeoDa)",
            ["b_value"] = "b-value",
            ["energy_index"] = "Energy Index",
        };

        foreach (var (key, name) in methodNames)
        {
            if (_trainResult.Baselines.TryGetValue(key, out var bl))
                rows.Add(new(name, bl.Accuracy.ToString("P1"), bl.F1Macro.ToString("F3")));
        }

        rows.Add(new("XGBoost (наша модель)",
            _trainResult.XgbAccuracy.ToString("P1"),
            _trainResult.XgbF1Macro.ToString("F3")));

        return rows;
    }

    private List<RobustnessRow> GetRobustnessRows()
    {
        if (_trainResult == null) return new();
        var rows = new List<RobustnessRow>();
        var labels = new Dictionary<string, string>
        {
            ["0.0"] = "0 (без шума)",
            ["0.5"] = "0.5 (~3x)",
            ["1.0"] = "1.0 (~10x)",
            ["1.5"] = "1.5 (~30x)",
        };
        foreach (var (sigma, label) in labels)
        {
            if (_trainResult.Robustness.TryGetValue(sigma, out var r))
                rows.Add(new(label, r.XgbF1.ToString("F3"), r.BaselineF1.ToString("F3")));
        }
        return rows;
    }
}

public record MetricRow(string Name, string Value);
public record CompareRow(string Method, string Accuracy, string F1Macro);
public record RobustnessRow(string Sigma, string XgbF1, string BaselineF1);
