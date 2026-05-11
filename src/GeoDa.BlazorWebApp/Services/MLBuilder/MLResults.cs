using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GeoDa.BlazorWebApp.Services.MLBuilder;

public class MLTrainResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public double XgbAccuracy { get; set; }
    public double XgbF1Macro { get; set; }
    public double XgbRocAuc { get; set; }
    public double XgbCvF1Mean { get; set; }

    public double IfAccuracy { get; set; }
    public double IfF1 { get; set; }

    public Dictionary<string, BaselineResult> Baselines { get; set; } = new();
    public Dictionary<string, RobustnessResult> Robustness { get; set; } = new();

    public static MLTrainResult Empty() => new() { Success = false, ErrorMessage = "Нет данных" };
    public static MLTrainResult Error(string msg) => new() { Success = false, ErrorMessage = msg };

    public static MLTrainResult FromJson(JsonElement root)
    {
        var result = new MLTrainResult { Success = true };

        if (root.TryGetProperty("xgboost", out var xgb))
        {
            result.XgbAccuracy = xgb.TryGetProperty("accuracy", out var v) ? v.GetDouble() : 0;
            result.XgbF1Macro = xgb.TryGetProperty("f1_macro", out v) ? v.GetDouble() : 0;
            result.XgbRocAuc = xgb.TryGetProperty("roc_auc_macro", out v) ? v.GetDouble() : 0;
            result.XgbCvF1Mean = xgb.TryGetProperty("cv_f1_macro_mean", out v) ? v.GetDouble() : 0;
        }

        if (root.TryGetProperty("isolation_forest", out var iso))
        {
            result.IfAccuracy = iso.TryGetProperty("accuracy_binary", out var v) ? v.GetDouble() : 0;
            result.IfF1 = iso.TryGetProperty("f1_anomaly", out v) ? v.GetDouble() : 0;
        }

        if (root.TryGetProperty("baseline_comparison", out var bl))
        {
            foreach (var prop in bl.EnumerateObject())
            {
                var br = new BaselineResult();
                if (prop.Value.TryGetProperty("accuracy", out var v)) br.Accuracy = v.GetDouble();
                if (prop.Value.TryGetProperty("f1_macro", out v)) br.F1Macro = v.GetDouble();
                result.Baselines[prop.Name] = br;
            }
        }

        if (root.TryGetProperty("robustness", out var rob))
        {
            foreach (var prop in rob.EnumerateObject())
            {
                var rr = new RobustnessResult();
                if (prop.Value.TryGetProperty("xgboost_f1_macro", out var v)) rr.XgbF1 = v.GetDouble();
                if (prop.Value.TryGetProperty("energy_threshold_f1_macro", out v)) rr.BaselineF1 = v.GetDouble();
                result.Robustness[prop.Name] = rr;
            }
        }

        return result;
    }
}

public class BaselineResult
{
    public double Accuracy { get; set; }
    public double F1Macro { get; set; }
}

public class RobustnessResult
{
    public double XgbF1 { get; set; }
    public double BaselineF1 { get; set; }
}

public class MLPredictResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalEvents { get; set; }
    public int AnomalyCount { get; set; }
    public int SlowAnomalyCount { get; set; }
    public Dictionary<string, int> ClassCounts { get; set; } = new();
    public List<PredictedEvent> Events { get; set; } = new();

    public static MLPredictResult Error(string msg) => new() { Success = false, ErrorMessage = msg };

    public static MLPredictResult FromFile(string path)
    {
        if (!File.Exists(path)) return Error("Файл predictions.json не найден");
        try
        {
            var json = File.ReadAllText(path);
            var doc = JsonDocument.Parse(json);
            var result = new MLPredictResult { Success = true };

            if (doc.RootElement.TryGetProperty("summary", out var summary))
            {
                result.TotalEvents = summary.TryGetProperty("total_events", out var v) ? v.GetInt32() : 0;
                result.AnomalyCount = summary.TryGetProperty("anomaly_count", out v) ? v.GetInt32() : 0;
                result.SlowAnomalyCount = summary.TryGetProperty("slow_anomaly_count", out v) ? v.GetInt32() : 0;
                if (summary.TryGetProperty("class_counts", out var cc))
                {
                    foreach (var p in cc.EnumerateObject())
                        result.ClassCounts[p.Name] = p.Value.GetInt32();
                }
            }

            if (doc.RootElement.TryGetProperty("events", out var events))
            {
                foreach (var e in events.EnumerateArray())
                {
                    result.Events.Add(new PredictedEvent
                    {
                        Index     = e.TryGetProperty("index",      out var v) ? v.GetInt32()  : 0,
                        Idat      = e.TryGetProperty("idat",       out v)     ? v.GetInt64()  : 0,
                        Itim      = e.TryGetProperty("itim",       out v)     ? v.GetInt64()  : 0,
                        X         = e.TryGetProperty("x",          out v)     ? v.GetDouble() : 0,
                        Y         = e.TryGetProperty("y",          out v)     ? v.GetDouble() : 0,
                        Z         = e.TryGetProperty("z",          out v)     ? v.GetDouble() : 0,
                        Energy    = e.TryGetProperty("e",          out v)     ? v.GetDouble() : 0,
                        Label     = e.TryGetProperty("label",      out v)     ? v.GetInt32()  : 0,
                        LabelName = e.TryGetProperty("label_name", out v)     ? v.GetString() ?? "" : "",
                        LabelColor= e.TryGetProperty("label_color",out v)     ? v.GetString() ?? "" : "",
                        IsAnomaly = e.TryGetProperty("is_anomaly", out v) && v.GetBoolean(),
                        AnomalyScore = e.TryGetProperty("anomaly_score", out v) ? v.GetDouble() : 0,
                        IsSlowAnomaly = e.TryGetProperty("is_slow_anomaly", out v) && v.GetBoolean(),
                    });
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}

public class PredictedEvent
{
    public int Index { get; set; }
    public long Idat { get; set; }
    public long Itim { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Energy { get; set; }
    public int Label { get; set; }
    public string LabelName { get; set; } = "";
    public string LabelColor { get; set; } = "";
    public bool IsAnomaly { get; set; }
    public double AnomalyScore { get; set; }
    public bool IsSlowAnomaly { get; set; }

    public string DateTimeStr
    {
        get
        {
            try
            {
                int year  = 2000 + (int)(Idat / 10000);
                int month = (int)(Idat / 100 % 100);
                int day   = (int)(Idat % 100);
                int hour  = (int)(Itim / 10000);
                int min   = (int)(Itim / 100 % 100);
                int sec   = (int)(Itim % 100);
                return $"{day:D2}.{month:D2}.{year} {hour:D2}:{min:D2}:{sec:D2}";
            }
            catch { return $"{Idat} / {Itim}"; }
        }
    }

    public string EnergyStr => Energy >= 1000
        ? $"{Energy / 1000:F1}k"
        : $"{Energy:F0}";

    public string AntdColor => LabelColor switch
    {
        "green"  => "success",
        "yellow" => "warning",
        "orange" => "orange",
        "red"    => "error",
        _        => "default"
    };
}
