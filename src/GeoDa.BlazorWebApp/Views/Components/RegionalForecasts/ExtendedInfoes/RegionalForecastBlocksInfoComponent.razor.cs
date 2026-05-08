using GeoDa.BlazorWebApp.Models.Gui.Colors;
using GeoDa.BlazorWebApp.Services.WebAppUtils;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using GeoDa.Domain.RegionalForecasts.Models.Settings.ParamFBlockSettings;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace GeoDa.BlazorWebApp.Views.Components.RegionalForecasts.ExtendedInfoes;

public partial class RegionalForecastBlocksInfoComponent
{
    [Parameter]
    public RfBlocksAssessment BlocksAssessment { get; set; } = default!;

    [Parameter]
    public RfParamFBlockSettings ParamFBlockSettings { get; set; } = default!;

    [Parameter]
    public ObjectStatus ObjectStatus { get; set; }

    // Private
    private string BuildLabel(
            ObjectStatus objectStatus,            
            string prefix,
            string str) =>
           prefix + objectStatus switch
           {
               ObjectStatus.Ok => str,
               ObjectStatus.DbError => "Ошибка подключения",
               ObjectStatus.Absent => "Нет объекта",
               ObjectStatus.HasDuplicate => "Дублирование объектов в базе",
               ObjectStatus.NotConnected => "Ошибка связи",
               ObjectStatus.Uncertain => "ObS: Uncertain",
               _ => "ObS: ERROR!"
           };

    private string BlockStatusToString(RfBlockStatus rfBlockStatus) =>
        rfBlockStatus switch
        {
            RfBlockStatus.Uncertain => "Неопределено",
            RfBlockStatus.LevelBackground => "Фоновый уровень",
            RfBlockStatus.Level1 => "Уровень 1",
            RfBlockStatus.Level2 => "Уровень 2",
            RfBlockStatus.Level3 => "Уровень 3",
            RfBlockStatus.Level4 => "Уровень 4",
            _ => "Неизвестное значение"
        };

    private string GetParamFStatusLabel()
    {
        var blockStatusString = BlockStatusToString(BlocksAssessment.ParamFStatus);
        var result = BuildLabel(ObjectStatus, "Макс. уровень по параметру F среди блоков: ", blockStatusString);
        return result;
    }

    private string GetCurrentEnergyStatusLabel()
    {
        var blockStatusString = BlockStatusToString(BlocksAssessment.CurrentEnergyStatus);
        var result = BuildLabel(ObjectStatus, "Макс. уровень по приведенной энергии среди блоков: ", blockStatusString);
        return result;
    }


    private GeoDaColor GetBgTextColor(RfBlockStatus blockStatus) =>
        ObjectStatus switch
        {
            ObjectStatus.Ok => blockStatus switch
            {
                RfBlockStatus.Uncertain => GeoDaColor.Blue,
                RfBlockStatus.LevelBackground => GeoDaColor.Blue,
                RfBlockStatus.Level1 => GeoDaColor.Green,
                RfBlockStatus.Level2 => GeoDaColor.Yellow,
                RfBlockStatus.Level3 => GeoDaColor.Orange,
                RfBlockStatus.Level4 => GeoDaColor.Red,
                _ => GeoDaColor.Blue
            },
            ObjectStatus.DbError => GeoDaColor.Yellow,
            ObjectStatus.Absent => GeoDaColor.Gray,
            ObjectStatus.HasDuplicate => GeoDaColor.Gray,
            ObjectStatus.NotConnected => GeoDaColor.Red,
            ObjectStatus.Uncertain => GeoDaColor.Blue,
            _ => GeoDaColor.Blue
        };

    private GeoDaColor GetFgTextColor(RfBlockStatus blockStatus) =>
       ObjectStatus switch
       {
           ObjectStatus.Ok => blockStatus switch
           {
               RfBlockStatus.Uncertain => GeoDaColor.White,
               RfBlockStatus.LevelBackground => GeoDaColor.White,
               RfBlockStatus.Level1 => GeoDaColor.White,
               RfBlockStatus.Level2 => GeoDaColor.Black,
               RfBlockStatus.Level3 => GeoDaColor.White,
               RfBlockStatus.Level4 => GeoDaColor.White,
               _ => GeoDaColor.Blue
           },
           ObjectStatus.DbError => GeoDaColor.Black,
           ObjectStatus.Absent => GeoDaColor.White,
           ObjectStatus.HasDuplicate => GeoDaColor.White,
           ObjectStatus.NotConnected => GeoDaColor.White,
           ObjectStatus.Uncertain => GeoDaColor.White,
           _ => GeoDaColor.White
       };

    private string GetBlockSize() =>
        $"Размер блока (м): {ParamFBlockSettings.ParamFCalcSettings.BlockSize}";

    private string GetTimeSpanMinImpactInDays() =>
        $"Минимальное влияние события (дни): {ParamFBlockSettings.ParamFCalcSettings.TimeSpanMinImpactInDays}";

    private string GetTimeSpanRegInDays() =>
        $"Временное окно регистрации (дни): {ParamFBlockSettings.ParamFCalcSettings.TimeSpanRegInDays}";

    private string GetBackgroundEnergyValue() =>
        $"Фоновый уровень энергии (Дж): {ParamFBlockSettings.ParamFCalcSettings.BackgroundEnergyValue}";
}
