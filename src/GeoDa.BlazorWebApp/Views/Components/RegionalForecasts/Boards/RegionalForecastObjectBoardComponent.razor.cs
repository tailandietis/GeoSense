using GeoDa.Application.RegionalForecasts.Repository.RegionalForecastActualDataStores;
using GeoDa.BlazorWebApp.Models.Gui.Colors;
using GeoDa.BlazorWebApp.Views.Pages;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Services.Factories;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;

namespace GeoDa.BlazorWebApp.Views.Components.RegionalForecasts.Boards;

public partial class RegionalForecastObjectBoardComponent
{
    [Inject]
    private ILogger<RegionalForecastObjectBoardComponent> Logger { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IRegionalForecastActualDataStore RfActualDataStore { get; set; } = default!;

    [Inject]
    private IEnergyAssessmentFactory EnergyAssessmentFactory { get; set; } = default!;

    [Inject]
    private IRegionalForecastBlocksInfoFactory RfBlocksInfoFactory { get; set; } = default!;

    [Parameter]
    public string ObjectName { get; set; } = string.Empty;

    private ObjectStatus _objectStatus = ObjectStatus.Uncertain;

    private EnergyAssessment _energyAssessment;
    private RegionalForecastBlocksInfo _rfBlocksInfo;

    public void Update()
    {
        try
        {
            (var objectEnergyStatus, _energyAssessment) = RfActualDataStore.GetEnergyAssessment(ObjectName);
            (var objectBlocksInfoStatus, _rfBlocksInfo) = RfActualDataStore.GetRfBlocksInfo(ObjectName);

            _objectStatus = (ObjectStatus)Math.Max((int)objectEnergyStatus, (int)objectBlocksInfoStatus);
        }
        catch(Exception ex)
        {
            Logger.LogError(exception: ex, message: ex.Message);

            _objectStatus = ObjectStatus.Uncertain;
            _energyAssessment = EnergyAssessmentFactory.CreateDefault();
            _rfBlocksInfo = RfBlocksInfoFactory.CreateDefault();
        }

        InvokeAsync(() => StateHasChanged());
    }

    // Override
    protected override void OnParametersSet()
    {
        Update();

        base.OnParametersSet();
    }

    // Private   

    // Popover buttons
    private void OpenInfoPage() =>
        NavigationManager.NavigateTo($"rf/{ObjectName}");

    private void OpenArchivePage() =>
        NavigationManager.NavigateTo($"archive/rf/{ObjectName}");

    // Card
    enum CardStatus
    {
        Uncertain,

        Normal,
        Warning,
        Alarm
    }

    private CardStatus GetCardStatus()
    {
        if(_objectStatus == ObjectStatus.Ok)
        {
            if (_energyAssessment.Report.Quality == Quality.Good)
            {
                if (_energyAssessment.Report.IsAlarm is true)
                    return CardStatus.Alarm;

                if(_rfBlocksInfo.BlocksAssessment.GeneralStatus == RfBlockStatus.Level4)
                    return CardStatus.Alarm;

                if (_rfBlocksInfo.BlocksAssessment.GeneralStatus == RfBlockStatus.Level3
                    || _rfBlocksInfo.BlocksAssessment.GeneralStatus == RfBlockStatus.Level2)
                    return CardStatus.Warning;

                return CardStatus.Normal;
            }

            return CardStatus.Uncertain;
        }
        return CardStatus.Uncertain;
    }

    private GeoDaColor GetCardBgColor()
    {
        var result = _objectStatus switch
        {
            ObjectStatus.Ok => _energyAssessment.Report.Quality switch
            {
                Quality.Good => GetCardStatus() switch
                {
                    CardStatus.Uncertain => GeoDaColor.Blue,
                    CardStatus.Normal => GeoDaColor.Green,
                    CardStatus.Warning => GeoDaColor.Orange,
                    CardStatus.Alarm => GeoDaColor.Red,
                    _ => GeoDaColor.Blue
                },
                Quality.Bad => GeoDaColor.Red,
                _ => GeoDaColor.Blue
            },
            ObjectStatus.DbError => GeoDaColor.Yellow,
            ObjectStatus.Absent => GeoDaColor.Gray,
            ObjectStatus.HasDuplicate => GeoDaColor.Gray,
            ObjectStatus.NotConnected => GeoDaColor.Red,
            ObjectStatus.Uncertain => GeoDaColor.Blue,
            _ => GeoDaColor.Blue
        };

        return result;
    }

    private GeoDaColor GetTextFgColor() =>
        _objectStatus switch
        {
            ObjectStatus.Ok => GeoDaColor.White,
            ObjectStatus.DbError => GeoDaColor.Black,
            ObjectStatus.Absent => GeoDaColor.White,
            ObjectStatus.HasDuplicate => GeoDaColor.White,
            ObjectStatus.NotConnected => GeoDaColor.White,
            ObjectStatus.Uncertain => GeoDaColor.White,
            _ => GeoDaColor.White
        };

    private string GetObjectNameLabel() =>
        $"Объект: {ObjectName}";
    
    private string GetEnergyStatusLabel() =>
        _objectStatus switch
        {
            ObjectStatus.Ok => _energyAssessment.Report.Quality switch
            {
                Quality.Good => _energyAssessment.Report.IsAlarm switch
                {
                    true => "Сейсм. энергия: ОПАСНО!",
                    false => "Сейсм. энергия < Порог",
                },
                Quality.Bad => "Не определено!",
                _ => "PS: ERROR!"
            },
            ObjectStatus.DbError => "Ошибка подкл.",
            ObjectStatus.Absent => "Нет объекта",
            ObjectStatus.HasDuplicate => "Дублир. об.",
            ObjectStatus.NotConnected => "Ошибка связи",
            ObjectStatus.Uncertain => "ObS: Uncertain",
            _ => "ObS: ERROR!"
        };

    private string GetELimitLabel() =>
         "Порог(Дж): " + _objectStatus switch
         {
             ObjectStatus.Ok => _energyAssessment.Object.EnergyLimit.ToString("0.00", CultureInfo.InvariantCulture),
             ObjectStatus.DbError => "Ошибка подкл.",
             ObjectStatus.Absent => "Нет объекта",
             ObjectStatus.HasDuplicate => "Дублир. об.",
             ObjectStatus.NotConnected => "Ошибка связи",
             ObjectStatus.Uncertain => "ObS: Uncertain",
             _ => "ObS: ERROR!"
         };

    private string GetBlocksInfoLabel() =>
        _objectStatus switch
        {
            ObjectStatus.Ok => _rfBlocksInfo.BlocksAssessment.GeneralStatus switch
            {
                RfBlockStatus.Uncertain  => "ObS: Uncertain",
                RfBlockStatus.LevelBackground => "Сейсм. акт.: фоновый уровень",
                RfBlockStatus.Level1 => "Сейсм. акт.: Уровень 1",
                RfBlockStatus.Level2 => "Сейсм. акт.: Уровень 2",
                RfBlockStatus.Level3 => "Сейсм. акт.: Уровень 3",
                RfBlockStatus.Level4 => "Сейсм. акт.: Уровень 4",
                _ => "PS: ERROR!"
            },
            ObjectStatus.DbError => "Ошибка подкл.",
            ObjectStatus.Absent => "Нет объекта",
            ObjectStatus.HasDuplicate => "Дублир. об.",
            ObjectStatus.NotConnected => "Ошибка связи",
            ObjectStatus.Uncertain => "ObS: Uncertain",
            _ => "ObS: ERROR!"
        };

    private string GetObjectStatusLabelId() =>
        $"{ObjectName}_rf_status_label";

    private string GetELimitLabelId() =>
        $"{ObjectName}_rf_e_limit_label";

    private string GetCardId() =>
        $"{ObjectName}_rf_card";
}
