using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GeoDa.Application.RegionalForecasts.Services.RfBlocksInfoMaps;

public partial class RfBlocksInfoMapsService
{
    private void ClearDataDirectory(int objectId, string dirName)
    {
        var dirInfo = new DirectoryInfo(dirName);

        foreach (var f in dirInfo.GetFiles())
        {
            if (f.Name.Contains($"blocks_info_{objectId}_")
                || f.Name.Contains($"map_settings_bi_{objectId}_"))
                f.Delete();
        }
    }

    private void CreateParamFBlocksInfoDataFile(string dataFileName, IReadOnlyList<ParamFBlockInfo> data)
    {
        var blockInfoRowToString = (ParamFBlockInfo x) => $"{x.X} {x.Y} " +
            $"{x.ParamF.ToString("0.0000", CultureInfo.InvariantCulture)}";

        var stringForWrite = new List<string> { "X Y F" };

        stringForWrite.AddRange(data.Select(x => blockInfoRowToString(x)));

        File.WriteAllLines(dataFileName, stringForWrite);
    }

    private void CreateCurrentEnergyBlocksInfoDataFile(string dataFileName, IReadOnlyList<CurrentEnergyBlockInfo> data)
    {
        var blockInfoRowToString = (CurrentEnergyBlockInfo x) => $"{x.X} {x.Y} " +
            $"{x.MaxCurrentEnergy.ToString("0.0000", CultureInfo.InvariantCulture)}";

        var stringForWrite = new List<string> { "X Y E" };

        stringForWrite.AddRange(data.Select(x => blockInfoRowToString(x)));

        File.WriteAllLines(dataFileName, stringForWrite);
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
            mine_map_width = rfSettings.ParamFBlockSettings.FieldSettings.FieldSize.X,
            mine_map_height = rfSettings.ParamFBlockSettings.FieldSettings.FieldSize.Y,            
            layout = rfSettings.LayoutFileName,
            param_f_map_file_name = images[0],
            total_energy_map_file_name = images[1],
            f_background = rfSettings.ParamFBlockSettings.EstimationSettings.FBackgound,
            e_background = rfSettings.ParamFBlockSettings.EstimationSettings.ECurrentBackground,
            f_level1 = rfSettings.ParamFBlockSettings.EstimationSettings.FLevel1,
            e_level1 = rfSettings.ParamFBlockSettings.EstimationSettings.ECurrentLevel1,
            f_level2 = rfSettings.ParamFBlockSettings.EstimationSettings.FLevel2,
            e_level2 = rfSettings.ParamFBlockSettings.EstimationSettings.ECurrentLevel2,
            f_level3 = rfSettings.ParamFBlockSettings.EstimationSettings.FLevel3,
            e_level3 = rfSettings.ParamFBlockSettings.EstimationSettings.ECurrentLevel3
        };

        _jsonFileService.Save(mapSettingsFileName, tmpSettings);
    }

    private void BuildMapsImages(
        string paramFBlocksInfoFileName,
        string currentEnergyBlocksInfoFileName,
        string mapSettingsFileName)
    {
        var args = $" -f {paramFBlocksInfoFileName} -e {currentEnergyBlocksInfoFileName} -s {mapSettingsFileName}";

        ProcessStartInfo startInfo = new ProcessStartInfo();
                
        startInfo.UseShellExecute = false;
        startInfo.WindowStyle = ProcessWindowStyle.Normal;

        startInfo.FileName = Path.Combine("paramfbuilder", "param_f_builder.exe");        
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
            if ((f.Name.Contains($"param_f_map_{objectId}")
                || f.Name.Contains($"current_e_map_{objectId}"))
                && imgsNotForDel.Select(v => Path.GetFileName(v)).All(v => v != f.Name))
            {
                f.Delete();
            }
        }
    }
}
