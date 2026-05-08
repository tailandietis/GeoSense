using GeoDa.Application.AlarmMonitors.Services.AlarmMonitorServices;
using GeoDa.Application.GeneralForecasts.Services.Utils;
using GeoDa.Application.RegionalForecasts.Extensions;
using GeoDa.Application.RegionalForecasts.Repository.GsAlarms;
using GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies;
using GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies.Dtos;
using GeoDa.Application.RegionalForecasts.Repository.GsStats;
using GeoDa.Application.RegionalForecasts.Repository.RegionalForecastActualDataStores;
using GeoDa.Application.RegionalForecasts.Services;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using GeoDa.Domain.RegionalForecasts.Services.Factories;
using GeoDa.Infrastructure.Services.DateTimes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GeoDa.Application.RegionalForecasts.HostedServices;

internal sealed class RegionalForecastHostedService : BackgroundService
{
    private const int CountMillisecondsInSecond = 1000;

    private readonly IGsAlarmRepository _gsAlarmRepository;
    private readonly IGsStatRepository _gsStatRepository;
    private readonly IGsParamFEnergyRepository _gsParamFEnergyRepository;

    private readonly IAlarmMonitorService _alarmMonitorService;

    private readonly IRegionalForecastService _rfService;
    private readonly IRegionalForecastActualDataStore _rfActualDataStore;

    private readonly IEnergyAssessmentFactory _energyAssessmentFactory;
    private readonly IObjectStatisticsFactory _objectStatisticsFactory;

    private readonly IGeneralForecastUtilsService _rfUtilsService;

    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<RegionalForecastHostedService> _logger;

    private readonly RegionalForecastSettings _rfSettings;

    private bool _isEnergyAssessmentFirstRun = true;
    private bool _isParamFCalcFirstRun = true;

    public RegionalForecastHostedService(
        IGsAlarmRepository gsAlarmRepository,
        IGsStatRepository gsStatRepository,
        IGsParamFEnergyRepository gsParamFEnergyRepository,
        IAlarmMonitorService alarmMonitorService,
        IRegionalForecastService rfService,
        IRegionalForecastActualDataStore rfActualDataStore,
        IEnergyAssessmentFactory energyAssessmentFactory,
        IObjectStatisticsFactory objectStatisticsFactory,
        IGeneralForecastUtilsService rfUtils,
        IDateTimeService dateTimeService,
        IOptions<RegionalForecastSettings> rfSettings,
        ILogger<RegionalForecastHostedService> logger)
    {
        _gsAlarmRepository = gsAlarmRepository;
        _gsStatRepository = gsStatRepository;
        _gsParamFEnergyRepository = gsParamFEnergyRepository;

        _alarmMonitorService = alarmMonitorService;
        _rfService = rfService;
        _rfActualDataStore = rfActualDataStore;
        _energyAssessmentFactory = energyAssessmentFactory;
        _objectStatisticsFactory = objectStatisticsFactory;

        _rfUtilsService = rfUtils;
        _dateTimeService = dateTimeService;

        _logger = logger;
        _rfSettings = rfSettings.Value;

        var dbConnString = _rfUtilsService.BuildDbConnectionString(_rfSettings.DbSettings.DbName);
        _gsAlarmRepository.DbConnectionString = dbConnString;
        _gsStatRepository.DbConnectionString = dbConnString;
        _gsParamFEnergyRepository.DbConnectionString = dbConnString;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        await Task.Yield();

        var alarmEnergyCheckPause = GetAlarmEnergyCheckUpdatePeriodInMilliseconds();
        var statCalcPause = GetStatCalcUpdatePeriodInMilliseconds();
        var paramFCalcPause = GetParamFCalcUpdatePauseInMilliseconds();

        var energyAssessmentWorker = Worker(EnergyAssessmentProcessing, alarmEnergyCheckPause, token);
        var statCalcWorker = Worker(StatCalcProcessing, statCalcPause, token);
        var paramFCalcWorker = Worker(ParamFCalcProcessing, paramFCalcPause, token);

        var rfTasks = new List<Task>()
        {
            energyAssessmentWorker, 
            statCalcWorker, 
            paramFCalcWorker
        };

        await Task.WhenAll(rfTasks);
    }

    // Private

    private async Task Worker(
        Func<string, DateTime, CancellationToken, Task> worker,
        int pause,
        CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var currentDt = _dateTimeService.GetCurrentDateTime();

            var tasks = new List<Task>();

            foreach (var rfObjectName in _rfSettings.ObjectsSettings.Keys)
            {
                var task = worker(rfObjectName, currentDt, token);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            await Task.Delay(pause, token);
        }
    }

    // EnergyAssessmentProcessing
    private Task EnergyAssessmentProcessing(string objectName, DateTime dateTime, CancellationToken stoppingToken)
    {
        try
        {
            (var objectStatus, var gpCountInfo) = _rfService.GetGpnsCountInfoAtSpecificTime(objectName, dateTime);

            if (objectStatus == ObjectStatus.Ok && gpCountInfo.Quality == Quality.Good)
            {
                _rfActualDataStore.UpdateGpnInfo(objectName, objectStatus, gpCountInfo);

(objectStatus, var energyAssessment) = _rfService.GetEnergyAssessment(objectName, dateTime);

                if (energyAssessment.Object.ObjectId >= 0)
                {
                    UpdateAlarmMonitorForEnergyAssessment(energyAssessment.Object.ObjectId, objectName,
                        energyAssessment.Report.IsAlarm);

                    if (_isEnergyAssessmentFirstRun is false)
                        _rfService.BuildDensityMapsImages(energyAssessment);

                    // Write data to rf db
                    var gsAlarm = energyAssessment.ToGsAlarmDto();
                    _gsAlarmRepository.InsertGsAlarm(gsAlarm);
                }

                _rfActualDataStore.UpdateEnergyAssessment(objectName, objectStatus, energyAssessment);

if (_isEnergyAssessmentFirstRun)
                {
                    _rfService.BuildDensityMapsImages(energyAssessment);
                    _isEnergyAssessmentFirstRun = false;
                }
            }
            else
            {
                _rfActualDataStore.UpdateGpnInfo(objectName, ObjectStatus.NotConnected, gpCountInfo);

                _rfActualDataStore.UpdateEnergyAssessment(objectName,
                    ObjectStatus.NotConnected,
                    _energyAssessmentFactory.CreateDefault());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            _rfActualDataStore.UpdateGpnInfo(objectName, ObjectStatus.Uncertain, 
                new GpnsCountInfo(Quality.Uncertain, 0, DateTime.MinValue));

            _rfActualDataStore.UpdateEnergyAssessment(objectName,
                ObjectStatus.Uncertain,
                _energyAssessmentFactory.CreateDefault());
        }

        return Task.CompletedTask;
    }

    private void UpdateAlarmMonitorForEnergyAssessment(int objectId, string objectName, bool isAlarm)
    {
        var alarmId = _rfService.BuildEnergyAssessmentAlarmId(objectId);
        var alarmInfo = _alarmMonitorService.BuildActiveAlarmInfo(objectName, alarmId);

        if (isAlarm && _alarmMonitorService.IsAlarmExists(alarmId) is false)
            _alarmMonitorService.AddAlarm(alarmInfo);
        else if (isAlarm is false && _alarmMonitorService.IsAlarmExists(alarmId))
            _alarmMonitorService.RemoveActiveAlarm(alarmId);
    }

    // StatCalcProcessing
    private Task StatCalcProcessing(string objectName, DateTime dateTime, CancellationToken stoppingToken)
    {
        try
        {
            (var objectStatus, var data) = _rfService.GetObjectStatistics(objectName, dateTime);

            _rfActualDataStore.UpdateObjectStatistics(objectName, objectStatus, data);

            if (data.ObjectId >= 0)
            {
                var gsStat = data.ToGsStatDto();
                _gsStatRepository.InsertGsStat(gsStat);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            _rfActualDataStore.UpdateObjectStatistics(objectName,
                ObjectStatus.Uncertain,
                _objectStatisticsFactory.CreateDefault());
        }

        return Task.CompletedTask;
    }

    // ParamFCalcProcessing
    private async Task ParamFCalcProcessing(string objectName, DateTime dateTime, CancellationToken stoppingToken)
    {
        try
        {            
            (var objectStatus, var data) = await _rfService.GetRegionalForecastInfoAsync(objectName, dateTime);
                        
            if (objectStatus == ObjectStatus.Ok)
            {
                // Построение изображений
                _rfService.BuildBlocksInfoMapsImages(data);

                // Запись в БД
                var gsParamFEnergy = new GsParamFEnergyDto()
                {
                    Obj = data.ObjectId,
                    Dt = data.DtOfInfoPreparation,
                    ParamFStatus = (int)data.BlocksAssessment.ParamFStatus,
                    EnergyStatus = (int)data.BlocksAssessment.CurrentEnergyStatus,
                };               

                _gsParamFEnergyRepository.InsertGsParamFEnergy(gsParamFEnergy);
            }

            _rfActualDataStore.UpdateRfBlocksInfo(objectName, objectStatus, data);


        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            _rfActualDataStore.UpdateRfBlocksInfo(objectName, ObjectStatus.Uncertain,
                new RegionalForecastBlocksInfo());
        }
    }

    // Tools
    private int GetAlarmEnergyCheckUpdatePeriodInMilliseconds()
    {
        var result = _rfSettings.GeneralSettings.AlarmCheckUpdatePauseInSeconds;

        return result * CountMillisecondsInSecond;
    }

    private int GetStatCalcUpdatePeriodInMilliseconds()
    {
        var result = _rfSettings.GeneralSettings.StatCalcUpdatePauseInSeconds;

        return result * CountMillisecondsInSecond;
    }

    private int GetParamFCalcUpdatePauseInMilliseconds()
    {
        var result = _rfSettings.GeneralSettings.ParamFCalcUpdatePauseInSeconds;

        return result * CountMillisecondsInSecond;
    }
}
