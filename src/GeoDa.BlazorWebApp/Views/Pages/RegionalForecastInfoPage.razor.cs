using GeoDa.Application.RegionalForecasts.Repository.RegionalForecastActualDataStores;
using GeoDa.Application.RegionalForecasts.Services;
using GeoDa.Application.RegionalForecasts.Services.DensityMaps;
using GeoDa.Application.RegionalForecasts.Services.ParamFMaps;
using GeoDa.BlazorWebApp.Services.Observers;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using GeoDa.Domain.RegionalForecasts.Models.Settings.ParamFBlockSettings;
using GeoDa.Domain.RegionalForecasts.Services.Factories;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class RegionalForecastInfoPage : IObserverClient, IDisposable
{
    [Inject]
    private ILogger<RegionalForecastInfoPage> Logger { get; set; } = default!;

    [Inject]
    private IRegionalForecastActualDataStore RfActualDataStore { get; set; } = default!;

    [Inject]
    private IEnergyAssessmentFactory EnergyAssessmentFactory { get; set; } = default!;

    [Inject]
    private IObjectStatisticsFactory ObjectStatisticsFactory { get; set; } = default!;

    [Inject]
    private IObserverService ObserverService { get; set; } = default!;

    [Inject]
    private IRfDensityMapsService RfDensityMapsService { get; set; } = default!;

    [Inject]
    private IRfBlocksInfoMapsService RfBlocksInfoMapsService { get; set; } = default!;

    [Inject]
    private IRegionalForecastService RfService { get; set; } = default!;

    [Inject]
    private IOptions<RegionalForecastSettings> RfSettings { get; set; } = default!;

    [Parameter]
    public string ObjectName { get; set; } = string.Empty;

    private (ObjectStatus Status, EnergyAssessment Data) _energyAssessment = new();

    private (ObjectStatus ObjectStatus, List<Event> Data) _alarmEvents = new();

    private (ObjectStatus Status, GpnsCountInfo Data) _gpnsCountInfo = new();

    private (ObjectStatus Status, ObjectStatistics Data) _objectStatistics = new();

    private (ObjectStatus Status, RegionalForecastBlocksInfo Data) _rfBlocksInfo = new();

    private string _densityEventsMapImagePath = string.Empty;

    private string _densityEventsWithLimitMapImagePath = string.Empty;

    private string _densityEnergyMapImagePath = string.Empty;

    private string _paramFMapImagePath = string.Empty;

    private string _totalEnergyMapImagePath = string.Empty;

    // Override
    protected override void OnInitialized()
    {
        if (!ObserverService.IsRegistered(this))
            ObserverService.RegisterObserver(this);

        UpdateData();

        base.OnInitialized();
    }

    // IObserverClient
    public void Update(object data)
    {
        //var msg = (MessageDataItem)data;

        //if (msg.SourceName == RfConsts.MODULE_TYPE_NAME)
        //{
        //    (var status, var rfData) = RfService.GetRegionalForecastObjectData(ObjectName);
        //    if (status == OpStatus.OK)
        //        RfData = rfData;
        //    else
        //        RfData = new();
        //}

        UpdateData();

        InvokeAsync(() => StateHasChanged());
    }

    // IDisposable
    public void Dispose() =>
        ObserverService.RemoveObserver(this);

    // Private
    private void UpdateData()
    {
        try
        {
            _energyAssessment = RfActualDataStore.GetEnergyAssessment(ObjectName);

            if(_energyAssessment.Status == ObjectStatus.Ok && _energyAssessment.Data.Report.IsAlarm is true)
            {
                _alarmEvents = RfService.GetLastAlarmEvents(ObjectName, 
                    _energyAssessment.Data.Object.DateTimeOfAssessment, 5);
            }
            else
            {
                _alarmEvents = (_energyAssessment.Status, new());
            }

            _gpnsCountInfo = RfActualDataStore.GetGpnInfo(ObjectName);            
            _rfBlocksInfo = RfActualDataStore.GetRfBlocksInfo(ObjectName);

            var densityMaps = RfDensityMapsService.GetDensityMapsImagesName(_energyAssessment.Data.Object.ObjectId, 
                _energyAssessment.Data.Object.DateTimeOfAssessment);

            _densityEventsMapImagePath = Path.Combine("img", densityMaps[DensityMapType.EventsCount]);
            _densityEventsWithLimitMapImagePath = Path.Combine("img", densityMaps[DensityMapType.EventsCountWithLimit]);
            _densityEnergyMapImagePath = Path.Combine("img", densityMaps[DensityMapType.Energy]);

            if(_rfBlocksInfo.Status == ObjectStatus.Ok && _rfBlocksInfo.Data != null)
            {
                var blocksInfoMaps = RfBlocksInfoMapsService.GetBlocksInfoMapsImagesName(_rfBlocksInfo.Data.ObjectId,
                    _rfBlocksInfo.Data.DtOfInfoPreparation);

                _paramFMapImagePath = Path.Combine("img", blocksInfoMaps[BlocksInfoMapType.ParamFMap]);
                _totalEnergyMapImagePath = Path.Combine("img", blocksInfoMaps[BlocksInfoMapType.CurrentEnergyMap]);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(exception: ex, message: ex.Message);

            _energyAssessment = (ObjectStatus.Uncertain, EnergyAssessmentFactory.CreateDefault());
            _gpnsCountInfo = (ObjectStatus.Uncertain, new GpnsCountInfo(Quality.Bad, 0, DateTime.MinValue));

            _densityEventsMapImagePath = string.Empty;
            _densityEventsWithLimitMapImagePath = string.Empty;
            _densityEnergyMapImagePath = string.Empty;

            _paramFMapImagePath = string.Empty;
            _totalEnergyMapImagePath = string.Empty;
        }

        try
        {
            _objectStatistics = RfActualDataStore.GetObjectStatistics(ObjectName);
        }
        catch (Exception ex)
        {
            Logger.LogError(exception: ex, message: ex.Message);

            _objectStatistics = (ObjectStatus.Uncertain, ObjectStatisticsFactory.CreateDefault());
        }

    }

    private RfParamFBlockSettings GetObjectBlocksSettings()
    {
        var result = RfSettings.Value.ObjectsSettings[ObjectName].ParamFBlockSettings;
        return result;
    }
}
