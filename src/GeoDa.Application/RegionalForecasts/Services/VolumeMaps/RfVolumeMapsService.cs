using GeoDa.Application.GeneralForecasts.Repository.Geometries;
using GeoDa.Application.GeneralForecasts.Repository.Geometries.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.Geophones;
using GeoDa.Application.GeneralForecasts.Repository.Geophones.Dtos;
using GeoDa.Application.GeneralForecasts.Services.Utils;
using GeoDa.Domain.RegionalForecasts.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace GeoDa.Application.RegionalForecasts.Services.VolumeMaps;

public class RfVolumeMapsService : IRfVolumeMapsService
{
    private const int DefaultEmissionLevel = 100;
    private readonly ILogger<RfVolumeMapsService> _logger;
    private readonly VolumetricBuilderSettings _settings;
    private readonly IGeometryRepository _geometryRepository;
    private readonly IGeophoneRepository _geophoneRepository;

    public RfVolumeMapsService(
        ILogger<RfVolumeMapsService> logger,
        IOptions<VolumetricBuilderSettings> settings,
        IGeometryRepository geometryRepository,
        IGeophoneRepository geophoneRepository,
        IGeneralForecastUtilsService gfUtils)
    {
        _logger = logger;
        _settings = settings.Value;
        _geometryRepository = geometryRepository;
        _geophoneRepository = geophoneRepository;

        var dbConnString = gfUtils.BuildDbConnectionString("DbPeleng");
        _geometryRepository.DbConnectionString = dbConnString;
        _geophoneRepository.DbConnectionString = dbConnString;
    }

    public string? CreateVolumeMap(int objectId, IReadOnlyList<Event> events, IReadOnlyList<VolumeLayoutConfig>? layouts = null)
    {
        var validLayouts = new List<VolumeLayoutConfig>();
        foreach (var layout in layouts ?? Array.Empty<VolumeLayoutConfig>())
        {
            if (File.Exists(layout.FilePath))
                validLayouts.Add(layout);
            else
                _logger.LogWarning("Layout file not found, skipping: {FilePath}", layout.FilePath);
        }

        try
        {
            var builderDir    = _settings.BuilderDir;
            var dataDir       = Path.Combine(builderDir, "data");
            var settingsDir   = Path.Combine(builderDir, "settings");
            var builderImgDir = Path.Combine(builderDir, "wwwroot", "img");

            Directory.CreateDirectory(dataDir);
            Directory.CreateDirectory(settingsDir);
            Directory.CreateDirectory(builderImgDir);

            var timestamp    = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var htmlFileName = $"events_volume_{objectId}_{timestamp}.html";

            ClearPreviousFiles(objectId, dataDir, settingsDir, builderImgDir);

            var eventsFilePath   = Path.GetFullPath(Path.Combine(dataDir,     $"events_{objectId}_{timestamp}.csv"));
            var settingsFilePath = Path.GetFullPath(Path.Combine(settingsDir, $"volume_settings_{objectId}_{timestamp}.json"));

            WriteEventsFile(eventsFilePath, events);

            var geometry  = _geometryRepository.SelectGeometryData(objectId);
            var geophones = _geophoneRepository.SelectAllGeophonsData(objectId);

            if (geophones == null || geophones.Count == 0)
            {
                _logger.LogError("Geophones not found for objectId: {ObjectId}", objectId);
                return null;
            }

            WriteSettingsFile(settingsFilePath, htmlFileName, builderDir, geometry, geophones, validLayouts);

            var buildResult = RunBuilder(eventsFilePath, settingsFilePath, builderDir);
            if (!buildResult.IsSuccess)
            {
                _logger.LogError("volume_builder error: {Error}", buildResult.ErrorMessage);
                return null;
            }

            var builtHtmlPath = Path.Combine(builderImgDir, htmlFileName);
            if (!File.Exists(builtHtmlPath))
            {
                _logger.LogError("HTML file not found after build: {Path}", builtHtmlPath);
                return null;
            }

            var webRootImgDir = _settings.WebRootImgPath;
            Directory.CreateDirectory(webRootImgDir);
            File.Copy(builtHtmlPath, Path.Combine(webRootImgDir, htmlFileName), overwrite: true);

            return $"/img/{htmlFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in CreateVolumeMap for objectId: {ObjectId}", objectId);
            return null;
        }
    }

    // Private helpers

    private void WriteEventsFile(string filePath, IReadOnlyList<Event> events)
    {
        var lines = new List<string>(events.Count + 1) { "D T X Y Z E" };

        lines.AddRange(events.Select(e =>
            $"{e.Dt:dd.MM.yy} {e.Dt:HH:mm:ss} " +
            $"{(int)Math.Round((double)e.X)} {(int)Math.Round((double)e.Y)} {(int)Math.Round((double)e.Z)} " +
            $"{e.E.ToString("0.0000", CultureInfo.InvariantCulture)}"));

        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }

    private void WriteSettingsFile(
        string filePath,
        string htmlFileName,
        string builderDir,
        GeometryDto geometry,
        List<GeophoneDto> geophones,
        IReadOnlyList<VolumeLayoutConfig> layouts)
    {
        var geophonesDict = geophones
            .Select((g, i) => new { Key = $"geophone_{i + 1:D2}", g })
            .ToDictionary(
                x => x.Key,
                x => (object)new { x = (double)x.g.X, y = (double)x.g.Y, z = (double)x.g.Z });

        var zMin = (int)geometry.ZMin;
        var layoutsData = layouts.Select(l =>
        {
            var zCoor = l.ZCoordinate ?? zMin;
            if (l.ZCoordinate is null)
                _logger.LogInformation("VolumeLayoutZCoor not set for layout {FilePath}, using z_min: {ZMin}", l.FilePath, zMin);

            return new { file_name = l.FilePath, z_coor = zCoor, opacity = l.Opacity };
        }).ToList();

        var settings = new
        {
            mine_volume_x0     = (int)geometry.XMin,
            mine_volume_y0     = (int)geometry.YMin,
            mine_volume_z0     = (int)geometry.ZMin,
            mine_volume_height = (int)(geometry.XMax - geometry.XMin),
            mine_volume_width  = (int)(geometry.YMax - geometry.YMin),
            mine_volume_length = (int)(geometry.ZMax - geometry.ZMin),
            e_emission_level   = DefaultEmissionLevel,
            volume_layouts                     = layoutsData,
            events_volume_file_name            = Path.GetFullPath(Path.Combine(builderDir, "wwwroot", "img", htmlFileName)),
            events_volume_with_limit_file_name = string.Empty,
            geophones_coordinates              = geophonesDict
        };

        File.WriteAllText(filePath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }

    private (bool IsSuccess, string ErrorMessage) RunBuilder(string eventsFilePath, string settingsFilePath, string builderDir)
    {
        var exePath = Path.Combine(builderDir, "volume_builder.exe");

        if (!File.Exists(exePath))
            return (false, $"volume_builder.exe не найден: {exePath}");

        var startInfo = new ProcessStartInfo
        {
            FileName               = exePath,
            Arguments              = $"-e \"{eventsFilePath}\" -s \"{settingsFilePath}\"",
            WorkingDirectory       = builderDir,
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow         = true
        };

        try
        {
            using var process = Process.Start(startInfo);

            if (process is null)
                return (false, "Не удалось запустить процесс volume_builder");

            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode == 0
                ? (true, string.Empty)
                : (false, $"ExitCode {process.ExitCode}: {error}");
        }
        catch (Exception ex)
        {
            return (false, $"Exception: {ex.Message}");
        }
    }

    private static void ClearPreviousFiles(int objectId, string dataDir, string settingsDir, string imgDir)
    {
        DeleteMatchingFiles(dataDir,     $"events_{objectId}_",          ".csv");
        DeleteMatchingFiles(settingsDir, $"volume_settings_{objectId}_", ".json");
        DeleteMatchingFiles(imgDir,      $"events_volume_{objectId}_",   ".html");
    }

    private static void DeleteMatchingFiles(string dir, string prefix, string extension)
    {
        if (!Directory.Exists(dir))
            return;

        foreach (var file in new DirectoryInfo(dir).GetFiles())
        {
            if (file.Name.StartsWith(prefix, StringComparison.Ordinal)
                && file.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                file.Delete();
            }
        }
    }
}
