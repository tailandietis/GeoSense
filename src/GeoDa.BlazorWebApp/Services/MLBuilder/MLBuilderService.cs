using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Services.MLBuilder;

public interface IMLBuilderService
{
    Task<MLTrainResult> TrainAsync(string objectName);
    Task<MLPredictResult> PredictAsync(string objectName, string eventsCsvPath);
    Task<MLPredictResult> PredictDemoAsync();
    bool IsTrainResultReady();
    bool IsPredictResultReady();
    MLTrainResult? GetLastTrainResult();
    MLPredictResult? GetLastPredictResult();
}

public class MLBuilderService : IMLBuilderService
{
    private readonly MLBuilderSettings _settings;
    private readonly ILogger<MLBuilderService> _logger;
    private MLTrainResult? _lastResult;
    private MLPredictResult? _lastPredictResult;

    public MLBuilderService(IOptions<MLBuilderSettings> settings, ILogger<MLBuilderService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<MLTrainResult> TrainAsync(string objectName)
    {
        var builderExe = Path.Combine(_settings.BuilderDir, "ml_builder.exe");
        var pythonScript = Path.Combine(_settings.BuilderDir, "ml_builder.py");
        var pythonExe = Path.Combine(_settings.BuilderDir, "venv", "Scripts", "python.exe");

        string executable;
        string arguments;

        if (File.Exists(builderExe))
        {
            executable = builderExe;
            arguments = $"--mode train --output \"{_settings.OutputDir}\"";
        }
        else
        {
            executable = pythonExe;
            arguments = $"\"{pythonScript}\" --mode train --output \"{_settings.OutputDir}\"";
            if (!string.IsNullOrEmpty(_settings.ThesisFiguresDir))
                arguments += $" --thesis_figures \"{_settings.ThesisFiguresDir}\"";
        }

        try
        {
            await RunProcessAsync(executable, arguments, _settings.BuilderDir);
            _lastResult = ReadMetrics();
            return _lastResult ?? MLTrainResult.Empty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка запуска mlbuilder");
            return MLTrainResult.Error(ex.Message);
        }
    }

    public async Task<MLPredictResult> PredictAsync(string objectName, string eventsCsvPath)
    {
        var builderExe = Path.Combine(_settings.BuilderDir, "ml_builder.exe");
        var pythonScript = Path.Combine(_settings.BuilderDir, "ml_builder.py");
        var pythonExe = Path.Combine(_settings.BuilderDir, "venv", "Scripts", "python.exe");

        var modelPath = Path.Combine(_settings.OutputDir, "xgb_model.json");
        var isoPath = Path.Combine(_settings.OutputDir, "iso_forest.pkl");
        var outPath = Path.Combine(_settings.OutputDir, "predictions.json");

        string executable;
        string arguments;

        if (File.Exists(builderExe))
        {
            executable = builderExe;
            arguments = $"--mode predict --events \"{eventsCsvPath}\" --model \"{modelPath}\" --iso_model \"{isoPath}\" --out \"{outPath}\"";
        }
        else
        {
            executable = pythonExe;
            arguments = $"\"{pythonScript}\" --mode predict --events \"{eventsCsvPath}\" --model \"{modelPath}\" --iso_model \"{isoPath}\" --out \"{outPath}\"";
        }

        try
        {
            await RunProcessAsync(executable, arguments, _settings.BuilderDir);
            return MLPredictResult.FromFile(outPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка predict режима");
            return MLPredictResult.Error(ex.Message);
        }
    }

    public async Task<MLPredictResult> PredictDemoAsync()
    {
        var demoEventsPath = Path.Combine(_settings.OutputDir, "demo_events.csv");
        if (!File.Exists(demoEventsPath))
            return MLPredictResult.Error("Демо-данные не найдены. Сначала обучите модель.");

        _lastPredictResult = await PredictAsync(string.Empty, demoEventsPath);
        return _lastPredictResult;
    }

    public bool IsTrainResultReady()
    {
        var metricsPath = Path.Combine(_settings.OutputDir, "metrics.json");
        return File.Exists(metricsPath);
    }

    public bool IsPredictResultReady()
    {
        var predictionsPath = Path.Combine(_settings.OutputDir, "predictions.json");
        return File.Exists(predictionsPath);
    }

    public MLTrainResult? GetLastTrainResult() => _lastResult ?? ReadMetrics();

    public MLPredictResult? GetLastPredictResult()
    {
        if (_lastPredictResult != null) return _lastPredictResult;
        var path = Path.Combine(_settings.OutputDir, "predictions.json");
        if (!File.Exists(path)) return null;
        _lastPredictResult = MLPredictResult.FromFile(path);
        return _lastPredictResult;
    }

    private MLTrainResult? ReadMetrics()
    {
        var metricsPath = Path.Combine(_settings.OutputDir, "metrics.json");
        if (!File.Exists(metricsPath)) return null;

        try
        {
            var json = File.ReadAllText(metricsPath);
            var doc = JsonDocument.Parse(json);
            return MLTrainResult.FromJson(doc.RootElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка чтения metrics.json");
            return null;
        }
    }

    private async Task RunProcessAsync(string executable, string arguments, string workDir)
    {
        var psi = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            WorkingDirectory = workDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        _logger.LogInformation("Запуск: {exe} {args}", executable, arguments);

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Не удалось запустить процесс");

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var stderr = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"mlbuilder завершился с кодом {process.ExitCode}: {stderr}");
        }
    }
}
