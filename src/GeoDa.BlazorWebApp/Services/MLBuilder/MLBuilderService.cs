using GeoDa.Application.GeneralForecasts.Services.Utils;
using GeoDa.Application.RegionalForecasts.Repository.Events;
using GeoDa.Application.RegionalForecasts.Repository.Events.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Services.MLBuilder;

public interface IMLBuilderService
{
    Task<MLTrainResult> TrainAsync(int objId);
    Task<MLPredictResult> PredictAsync(int objId, DateTime start, DateTime end);
    bool IsTrainResultReady();
    bool IsPredictResultReady();
    MLTrainResult? GetLastTrainResult();
    MLPredictResult? GetLastPredictResult();
}

public class MLBuilderService : IMLBuilderService
{
    private readonly MLBuilderSettings _settings;
    private readonly IEventRepository _eventRepository;
    private readonly IGeneralForecastUtilsService _utils;
    private readonly ILogger<MLBuilderService> _logger;
    private MLTrainResult? _lastResult;
    private MLPredictResult? _lastPredictResult;

    public MLBuilderService(
        IOptions<MLBuilderSettings> settings,
        IEventRepository eventRepository,
        IGeneralForecastUtilsService utils,
        ILogger<MLBuilderService> logger)
    {
        _settings = settings.Value;
        _eventRepository = eventRepository;
        _utils = utils;
        _logger = logger;
    }

    public async Task<MLTrainResult> TrainAsync(int objId)
    {
        var builderExe = Path.Combine(_settings.BuilderDir, "ml_builder.exe");
        var pythonScript = Path.Combine(_settings.BuilderDir, "ml_builder.py");
        var pythonExe = Path.Combine(_settings.BuilderDir, "venv", "Scripts", "python.exe");

        try
        {
            _eventRepository.DbConnectionString = _utils.BuildDbConnectionString("DbPeleng");
            var events = _eventRepository.SelectEventsAtRange(objId, new DateTime(2000, 1, 1), DateTime.Now);
            _logger.LogInformation("Загружено событий из БД для obj={Obj}: {Count}", objId, events.Count);

            var tempCsv = Path.Combine(_settings.OutputDir, "temp_events_train.csv");
            Directory.CreateDirectory(_settings.OutputDir);
            WriteEventsCsv(events, tempCsv);

            string executable;
            string arguments;

            if (File.Exists(builderExe))
            {
                executable = builderExe;
                arguments = $"--mode train --output \"{_settings.OutputDir}\" --real_events \"{tempCsv}\"";
            }
            else
            {
                executable = pythonExe;
                arguments = $"\"{pythonScript}\" --mode train --output \"{_settings.OutputDir}\" --real_events \"{tempCsv}\"";
                if (!string.IsNullOrEmpty(_settings.ThesisFiguresDir))
                    arguments += $" --thesis_figures \"{_settings.ThesisFiguresDir}\"";
            }

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

    public async Task<MLPredictResult> PredictAsync(int objId, DateTime start, DateTime end)
    {
        var builderExe = Path.Combine(_settings.BuilderDir, "ml_builder.exe");
        var pythonScript = Path.Combine(_settings.BuilderDir, "ml_builder.py");
        var pythonExe = Path.Combine(_settings.BuilderDir, "venv", "Scripts", "python.exe");

        var modelPath = Path.Combine(_settings.OutputDir, "xgb_model.json");
        var isoPath   = Path.Combine(_settings.OutputDir, "iso_forest.pkl");
        var outPath   = Path.Combine(_settings.OutputDir, "predictions.json");

        try
        {
            _eventRepository.DbConnectionString = _utils.BuildDbConnectionString("DbPeleng");
            var events = _eventRepository.SelectEventsAtRange(objId, start, end);
            _logger.LogInformation("Загружено событий из БД для predict obj={Obj}: {Count}", objId, events.Count);

            if (events.Count == 0)
                return MLPredictResult.Error("Нет событий в базе данных для выбранного объекта и периода.");

            var tempCsv = Path.Combine(_settings.OutputDir, "temp_events_predict.csv");
            Directory.CreateDirectory(_settings.OutputDir);
            WriteEventsCsv(events, tempCsv);

            string executable;
            string arguments;

            if (File.Exists(builderExe))
            {
                executable = builderExe;
                arguments = $"--mode predict --events \"{tempCsv}\" --model \"{modelPath}\" --iso_model \"{isoPath}\" --out \"{outPath}\"";
            }
            else
            {
                executable = pythonExe;
                arguments = $"\"{pythonScript}\" --mode predict --events \"{tempCsv}\" --model \"{modelPath}\" --iso_model \"{isoPath}\" --out \"{outPath}\"";
            }

            await RunProcessAsync(executable, arguments, _settings.BuilderDir);
            _lastPredictResult = MLPredictResult.FromFile(outPath);
            return _lastPredictResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка predict режима");
            return MLPredictResult.Error(ex.Message);
        }
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

    private static void WriteEventsCsv(List<EventDto> events, string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine("obj,idat,itim,n,x,y,z,e,magn,ampl,proc,np_actual,rq_min,rq_max");
        foreach (var ev in events)
        {
            sb.AppendLine(string.Join(",",
                ev.Obj,
                ev.Idat,
                ev.Itim,
                ev.N,
                (ev.X ?? 0).ToString(CultureInfo.InvariantCulture),
                (ev.Y ?? 0).ToString(CultureInfo.InvariantCulture),
                (ev.Z ?? 0).ToString(CultureInfo.InvariantCulture),
                FormatFloat(ev.E),
                FormatFloat(ev.Magn),
                FormatFloat(ev.Ampl),
                FormatFloat(ev.Proc),
                ev.NpActual.HasValue ? ev.NpActual.Value.ToString() : "",
                FormatFloat(ev.RqMin),
                FormatFloat(ev.RqMax)));
        }
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    private static string FormatFloat(float? value)
        => value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "";

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
