using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GeoDa.Application.RegionalForecasts.Services.DensityMaps;

public partial class RfDensityMapsService
{
    private void ClearDataDirectory(int objectId, string dirName)
    {
        var dirInfo = new DirectoryInfo(dirName);

        foreach (var f in dirInfo.GetFiles())
        {
            if (f.Name.Contains($"events_{objectId}_")
                || f.Name.Contains($"map_settings_{objectId}_"))
                f.Delete();
        }
    }

    private void CreateEventsFile(string eventsFileName, IReadOnlyList<Event> events)
    {
        var eventRowToString = (Event x) => $"{x.Dt:dd.MM.yy} {x.Dt:HH:mm:ss} {x.X} {x.Y} {x.Z} " +
            $"{x.E.ToString("0.0000", CultureInfo.InvariantCulture)}";

        var stringForWrite = new List<string> { "D T X Y Z E" };

        stringForWrite.AddRange(events.Select(x => eventRowToString(x)));

        File.WriteAllLines(eventsFileName, stringForWrite);
    }

    private void CreateMapSettingsFile(
        string mapSettingsFileName,
        IReadOnlyList<string> images,
        RegionalForecastObjectSettings rfSettings)
    {
        var tmpSettings = new
        { 
            mine_map_x0 = rfSettings.ParamFBlockSettings.FieldSettings.FieldCoords.X,
            mine_map_y0 = rfSettings.ParamFBlockSettings.FieldSettings.FieldCoords.Y,
            mine_map_height = rfSettings.ParamFBlockSettings.FieldSettings.FieldSize.X,
            mine_map_width = rfSettings.ParamFBlockSettings.FieldSettings.FieldSize.Y,
            block_count_at_height = rfSettings.CountOfBlockAtHeight,
            block_count_at_width = rfSettings.CountOfBlockAtWidth,
            e_emission_level = rfSettings.AlarmELevel,
            layout = rfSettings.LayoutFileName,
            events_map_file_name = images[0],
            events_map_with_limit_file_name = images[1],
            energy_map_file_name = images[2]
        };

        _jsonFileService.Save(mapSettingsFileName, tmpSettings);
    }

    private void BuildMapsImages(
        string eventsFileName,
        string mapSettingsFileName)
    {
        var args = $" -e {eventsFileName} -s {mapSettingsFileName}";

        ProcessStartInfo startInfo = new ProcessStartInfo();
                
        startInfo.UseShellExecute = false;
        startInfo.WindowStyle = ProcessWindowStyle.Normal;

        startInfo.FileName = Path.Combine("mapsbuilder","maps_builder.exe");        
        startInfo.Arguments = args;
                
        using Process? exeProcess = Process.Start(startInfo);
        
        if (exeProcess != null)
            exeProcess.WaitForExit();        
    }

    private void RemoveOldImg(int objectId, string imgDir, List<string> imgsNotForDel)
    {
        var dirInfo = new DirectoryInfo(imgDir);
        foreach (var f in dirInfo.GetFiles())
        {
            if ((f.Name.Contains($"events_map_{objectId}")
                || f.Name.Contains($"events_map_limit_{objectId}")
                || f.Name.Contains($"energy_map_limit_{objectId}"))
                && imgsNotForDel.Select(v => Path.GetFileName(v)).All(v => v != f.Name))
                f.Delete();
        }
    }
}
