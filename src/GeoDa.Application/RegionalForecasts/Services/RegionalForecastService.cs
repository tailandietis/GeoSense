using GeoDa.Application.Exceptions;
using GeoDa.Application.GeneralForecasts.Extensions;
using GeoDa.Application.GeneralForecasts.Repository.ErrorCodeTexts;
using GeoDa.Application.GeneralForecasts.Repository.MsgCodeTexts;
using GeoDa.Application.GeneralForecasts.Repository.MsgCodeTexts.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.MsgLogs;
using GeoDa.Application.GeneralForecasts.Repository.ObjectInfos;
using GeoDa.Application.GeneralForecasts.Repository.Qualsgrs;
using GeoDa.Application.GeneralForecasts.Services.Utils;
using GeoDa.Application.Objects;
using GeoDa.Application.RegionalForecasts.Extensions;
using GeoDa.Application.RegionalForecasts.Repository.Events;
using GeoDa.Application.RegionalForecasts.Repository.RegionalForecastActualDataStores;
using GeoDa.Application.RegionalForecasts.Services.DensityMaps;
using GeoDa.Application.RegionalForecasts.Services.ParamFMaps;
using GeoDa.Domain.AlarmMonitors.Models;
using GeoDa.Domain.CurrentForecasts.Services.SystemQualities;
using GeoDa.Domain.Exceptions;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.GeneralForecasts.Services.Objects;
using GeoDa.Domain.GeneralForecasts.Services.Qualsgrs;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using GeoDa.Domain.RegionalForecasts.Models.Settings.ParamFBlockSettings;
using GeoDa.Domain.RegionalForecasts.Services.EnergyAssessments;
using GeoDa.Domain.RegionalForecasts.Services.Factories;
using GeoDa.Domain.RegionalForecasts.Services.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Services.StatCalcs;
using GeoDa.Domain.Services.GeoDaUtils;
using GeoDa.Infrastructure.Services.DateTimes;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GeoDa.Application.RegionalForecasts.Services;

internal class RegionalForecastService : IRegionalForecastService
{
    private const int MinutesForWatch = 11;
    private const int ZeroEnergyLevel = 0;

    private readonly IEventRepository _eventRepository;
    private readonly IQualsgrRepository _qualsgrRepository;
    private readonly IMsgLogRepository _msgLogRepository;
    private readonly IMsgCodeTextRepository _msgCodeTextRepository;
    private readonly IErrorCodeTextRepository _errorCodeTextRepository;

    private readonly IRegionalForecastActualDataStore _rfActualDataStore;

    private readonly IEnergyAssessmentFactory _energyAssessmentFactory;
    private readonly IEnergyReportFactory _energyReportFactory;
    private readonly IEnergyReportObjectInfoFactory _energyReportObjectInfoFactory;
    private readonly IEventsStatisticsFactory _eventsStatisticsFactory;
    private readonly IObjectStatisticsFactory _objectStatisticsFactory;
    private readonly IRegionalForecastBlocksInfoFactory _rfBlocksInfoFactory;

    private readonly IQualsgrService _qualsgrService;
    private readonly IEnergyAssessmentService _energyAssessmentService;
    private readonly IStatCalcService _statCalcService;
    private readonly IRfDensityMapsService _rfDensityMapImageService;
    private readonly IRfBlocksInfoMapsService _rfBlocksInfoMapsService;
    private readonly IRfParamFBlockDomainService _rfParamFBlockDomainService;

    private readonly IGeneralForecastUtilsService _gfUtils;

    private readonly IObjectAppService _objectAppService;

    private readonly IGeoDaDomainUtils _geoDaDomainUtils;
    private readonly IDateTimeService _dateTimeService;

    private readonly ILogger<RegionalForecastService> _logger;

    private readonly RegionalForecastSettings _settings;
        
    private readonly Dictionary<int, string> _msgCodeText = new();
    private readonly Dictionary<int, string> _errorCodeText = new();

    public RegionalForecastService(
        IEventRepository eventRepository,
        IQualsgrRepository qualsgrRepository,
        IMsgLogRepository msgLogRepository,
        IMsgCodeTextRepository msgCodeTextRepository,
        IErrorCodeTextRepository errorCodeTextRepository,

        IRegionalForecastActualDataStore rfActualDataStore,

        IEnergyAssessmentFactory energyAssessmentFactory,
        IEnergyReportObjectInfoFactory energyReportObjectInfoFactory,
        IEnergyReportFactory energyReportFactory,
        IEventsStatisticsFactory eventsStatisticsFactory,
        IObjectStatisticsFactory objectStatisticsFactory,
        IRegionalForecastBlocksInfoFactory rfBlocksInfoFactory,

        IQualsgrService qualsgrService,
        IEnergyAssessmentService energyAssessmentService,
        IStatCalcService statCalcService,
        IRfDensityMapsService rfDensityMapImageService,
        IRfBlocksInfoMapsService rfBlocksInfoMapsService,
        IRfParamFBlockDomainService rfParamFBlockDomainService,

        IGeneralForecastUtilsService rfUtils,

        IObjectAppService objectAppService,

        IGeoDaDomainUtils geoDaDomainUtils,
        IDateTimeService dateTimeService,

        IOptions<RegionalForecastSettings> rfSettings,
        ILogger<RegionalForecastService> logger)
    {
        _eventRepository = eventRepository;
        _qualsgrRepository = qualsgrRepository;
        _msgLogRepository = msgLogRepository;
        _msgCodeTextRepository = msgCodeTextRepository;
        _errorCodeTextRepository = errorCodeTextRepository;

        _rfActualDataStore = rfActualDataStore;

        _qualsgrService = qualsgrService;
        _energyAssessmentFactory = energyAssessmentFactory;
        _energyReportObjectInfoFactory = energyReportObjectInfoFactory;
        _energyReportFactory = energyReportFactory;
        _eventsStatisticsFactory = eventsStatisticsFactory;
        _objectStatisticsFactory = objectStatisticsFactory;
        _rfBlocksInfoFactory = rfBlocksInfoFactory;

        _energyAssessmentService = energyAssessmentService;
        _statCalcService = statCalcService;
        _rfDensityMapImageService = rfDensityMapImageService;
        _rfBlocksInfoMapsService = rfBlocksInfoMapsService;
        _rfParamFBlockDomainService = rfParamFBlockDomainService;

        _gfUtils = rfUtils;

        _objectAppService = objectAppService;

        _geoDaDomainUtils = geoDaDomainUtils;
        _dateTimeService = dateTimeService;

        _logger = logger;

        _settings = rfSettings.Value;

        var dbConnString = _gfUtils.BuildDbConnectionString(_settings.DbSettings.DbName);
        _objectAppService.SetDbConnectionString(dbConnString);
        _eventRepository.DbConnectionString = dbConnString;
        _qualsgrRepository.DbConnectionString = dbConnString;
        _msgLogRepository.DbConnectionString = dbConnString;

        var dbConnStringGeoDaDb = _gfUtils.BuildDbConnectionString("DbGeoDA");
        _msgCodeTextRepository.DbConnectionString = dbConnStringGeoDaDb;
        _errorCodeTextRepository.DbConnectionString = dbConnStringGeoDaDb;
    }

    // IRegionalForecastService    
    public async Task<(ObjectStatus, RegionalForecastBlocksInfo)> GetRegionalForecastInfoAsync(
        string objectName,
        DateTime rfInfoCalcDateTime)
    {
        ArgumentNullException.ThrowIfNull(objectName);

        (var objectStatus, var objectId) = _objectAppService.GetObjectStatusAndId(objectName);

        if (objectStatus != ObjectStatus.Ok)
        {
            return (objectStatus, new RegionalForecastBlocksInfo());
        }

        if (_settings.ObjectsSettings.ContainsKey(objectName) is false)
        {
            return (ObjectStatus.Absent, new RegionalForecastBlocksInfo());
        }

        var settings = _settings.ObjectsSettings[objectName].ParamFBlockSettings;

        var start = rfInfoCalcDateTime.AddDays(-1 * settings.ParamFCalcSettings.TimeSpanRegInDays);
        var end = rfInfoCalcDateTime;

        var events = _eventRepository.SelectEventsAtRangeGreaterOrEqualToEnergyLevel(objectId, start, end, ZeroEnergyLevel)
                .Where(x => x.IsSomeProperitiesIsNull() is false)
                .Select(x => x.ToEvent(_geoDaDomainUtils.ConvertAzonFormatToDateTime))
                .ToList();

        var blocksInfo = await _rfParamFBlockDomainService.CalcRfInfoAsync(estimateDt: end, fieldSettings: settings.FieldSettings,
            fCalcSettings: settings.ParamFCalcSettings, estimationSettings: settings.EstimationSettings, seismoEvents: events);

        var blocksAssessment = _rfParamFBlockDomainService.CalcRfAllBlocksAssessment(blocksInfo);

        var rfInfo = new RegionalForecastBlocksInfo()
        {
            ObjectId = objectId,
            ObjectName = objectName,
            DtOfInfoPreparation = end,
            BlocksAssessment = blocksAssessment,
            BlocksInfo = blocksInfo,
        };

        return (ObjectStatus.Ok, rfInfo);
    }

    public async Task<(ObjectStatus, BlockInfo[,,])> GetRfBlocksInfoAsync(
        string objectName,
        DateTime rfInfoCalcDateTime,
        double blockSize,
        int timeSpanRegInDays,
        int timeSpanMinImpactInDays,
        double backgroundEnergy)
    {
        ArgumentNullException.ThrowIfNull(objectName);

        (var objectStatus, var objectId) = _objectAppService.GetObjectStatusAndId(objectName);

        if (objectStatus != ObjectStatus.Ok)
        {
            return (objectStatus, new BlockInfo[0, 0, 0]);
        }

        if (_settings.ObjectsSettings.ContainsKey(objectName) is false)
        {
            return (ObjectStatus.Absent, new BlockInfo[0, 0, 0]);
        }

        var fieldSettings = _settings.ObjectsSettings[objectName].ParamFBlockSettings.FieldSettings;

        var start = rfInfoCalcDateTime.AddDays(-1 * timeSpanRegInDays);
        var end = rfInfoCalcDateTime;

        var events = _eventRepository.SelectEventsAtRangeGreaterOrEqualToEnergyLevel(objectId, start, end, ZeroEnergyLevel)
                .Where(x => x.IsSomeProperitiesIsNull() is false)
                .Select(x => x.ToEvent(_geoDaDomainUtils.ConvertAzonFormatToDateTime))
                .ToList();

        var paramFCalcSettings = new RfParamFCalcSettings()
        {
            BlockSize = blockSize,
            TimeSpanRegInDays = timeSpanRegInDays,
            TimeSpanMinImpactInDays = timeSpanMinImpactInDays,
            BackgroundEnergyValue = backgroundEnergy
        };

        var estimationSettings = new RfSeismoActivityEstimationSettings();

        var blocksInfo = await _rfParamFBlockDomainService.CalcRfInfoAsync(estimateDt: end, fieldSettings: fieldSettings,
            fCalcSettings: paramFCalcSettings, estimationSettings: estimationSettings, seismoEvents: events);
        
        return (ObjectStatus.Ok, blocksInfo);
    }


    public (ObjectStatus, EnergyAssessment) GetEnergyAssessment(string objectName, DateTime dateTime)
    {
        var alarmEnergyLevel = _settings.ObjectsSettings[objectName].AlarmELevel;
        var checkInterval = _settings.ObjectsSettings[objectName].AlarmCheckIntervalInDays;

        var objectStatus = ObjectStatus.Uncertain;

        var energyReportObjectInfo = _energyReportObjectInfoFactory.CreateDefault() with
        {
            ObjectName = objectName,
            DateTimeOfAssessment = dateTime,
            EnergyLimit = alarmEnergyLevel,
            CheckIntervalInDays = checkInterval,
            MineMapWidth = _settings.ObjectsSettings[objectName].ParamFBlockSettings.FieldSettings.FieldSize.X,
            MineMapHeight = _settings.ObjectsSettings[objectName].ParamFBlockSettings.FieldSettings.FieldSize.Y,
            CountOfBlockAtWidth = _settings.ObjectsSettings[objectName].CountOfBlockAtWidth,
            CountOfBlockAtHeight = _settings.ObjectsSettings[objectName].CountOfBlockAtHeight
        };

        try
        {
            (objectStatus, var objectId) = _objectAppService.GetObjectStatusAndId(objectName);

            if (objectStatus != ObjectStatus.Ok)
            {
                var badEnergyAssessment = _energyAssessmentFactory.Create(energyReportObjectInfo,
                    _energyReportFactory.CreateDefault());

                return (objectStatus, badEnergyAssessment);
            }

            energyReportObjectInfo = energyReportObjectInfo with { ObjectId = objectId };

            (var start, var end) = _geoDaDomainUtils.CreateDateInterval(checkInterval, dateTime);

            var events = _eventRepository.SelectEventsAtRangeGreaterOrEqualToEnergyLevel(objectId, start, end, alarmEnergyLevel)
                .Where(x => x.IsSomeProperitiesIsNull() is false)
                .Select(x => x.ToEvent(_geoDaDomainUtils.ConvertAzonFormatToDateTime))
                .ToList();

            var energyReport = _energyAssessmentService.GetEnergyReport(events);

            var result = _energyAssessmentFactory.Create(energyReportObjectInfo, energyReport);

            return (ObjectStatus.Ok, result);

        }
        catch (GeoDaDomainException ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            var badEnergyAssessment = _energyAssessmentFactory.Create(energyReportObjectInfo,
                    _energyReportFactory.CreateDefault());

            return (objectStatus, badEnergyAssessment);
        }
        catch (GeoDaAppException ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            var status = _geoDaDomainUtils.BuildObjectStatusByOpStatus(ex.OperationStatus);

            var badEnergyAssessment = _energyAssessmentFactory.Create(energyReportObjectInfo,
                    _energyReportFactory.CreateDefault());

            return (status, badEnergyAssessment);
        }
    }

    public (ObjectStatus, List<Event>) GetLastAlarmEvents(string objectName, DateTime dateTime, int count)
    {
        var alarmEnergyLevel = _settings.ObjectsSettings[objectName].AlarmELevel;
        var checkInterval = _settings.ObjectsSettings[objectName].AlarmCheckIntervalInDays;

        var objectStatus = ObjectStatus.Uncertain;

        try
        {
            (objectStatus, var objectId) = _objectAppService.GetObjectStatusAndId(objectName);

            if (objectStatus != ObjectStatus.Ok)
            {                
                return (objectStatus, new List<Event>());
            }
            
            (var start, var end) = _geoDaDomainUtils.CreateDateInterval(checkInterval, dateTime);

            var events = _eventRepository.SelectEventsAtRangeGreaterOrEqualToEnergyLevel(objectId, start, end, alarmEnergyLevel)
                .Where(x => x.IsSomeProperitiesIsNull() is false)
                .Select(x => x.ToEvent(_geoDaDomainUtils.ConvertAzonFormatToDateTime))
                .OrderByDescending(x => x.Dt)
                .Take(count)
                .ToList();            

            return (ObjectStatus.Ok, events);

        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return (objectStatus, new List<Event>());
        }
    }

    public (ObjectStatus, GpnsCountInfo) GetGpnsCountInfoAtSpecificTime(string objectName, DateTime dateTime)
    {
        var objectStatus = ObjectStatus.Uncertain;

        var badGpnsCountInfo = new GpnsCountInfo(Quality.Uncertain, 0, DateTime.MinValue);

        try
        {
            (objectStatus, var objectId) = _objectAppService.GetObjectStatusAndId(objectName);

            if (objectStatus != ObjectStatus.Ok)
                return (objectStatus, badGpnsCountInfo);
                        

            var result = GetGpnsCountInfoAtSpecificTime(objectId, dateTime);
            
            return (objectStatus, result);
        }
        catch (GeoDaDomainException ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return (objectStatus, badGpnsCountInfo);
        }
        catch (GeoDaAppException ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            var status = BuildObjectStatusByOpStatus(ex.OperationStatus);
            return (status, badGpnsCountInfo);
        }
    }

    public AlarmId BuildEnergyAssessmentAlarmId(int objectId) =>
        _energyAssessmentService.BuildEnergyAssessmentAlarmId(objectId);

    public AlarmId BuildEnergyAssessmentAlarmId(string objectName)
    {
        (var objectStatus, var objectId) = _objectAppService.GetObjectStatusAndId(objectName);

        if (objectStatus != ObjectStatus.Ok)
            throw new GeoDaAppException(OpStatus.UnknownObjectName, $"Object: {objectName} is absent");

        var result = BuildEnergyAssessmentAlarmId(objectId);

        return result;
    }

    public (ObjectStatus, ObjectStatistics) GetObjectStatistics(string objectName, DateTime dateTime, int checkInterval)
    {
        var objectStatus = ObjectStatus.Uncertain;

        var objectStatistics = _objectStatisticsFactory.CreateDefault() with
        {
            DateTimeOfStatCalc = dateTime,
            StatCalcInterval = checkInterval
        };

        try
        {
            (objectStatus, var objectId) = _objectAppService.GetObjectStatusAndId(objectName);

            if (objectStatus != ObjectStatus.Ok)
                return (objectStatus, objectStatistics);

            objectStatistics = objectStatistics with { ObjectId = objectId };

            (var start, var end) = _geoDaDomainUtils.CreateDateInterval(checkInterval, dateTime);

            var events = _eventRepository.SelectEventsAtRange(objectId, start, end)
                .Where(x => x.IsSomeProperitiesIsNull() is false)
                .Select(x => x.ToEvent(_geoDaDomainUtils.ConvertAzonFormatToDateTime))
                .ToList();

            if (events.Count == 0)
            {
                _logger.LogWarning($"{objectName}: нет данных для построения статистики");

                var noDataResult = objectStatistics with
                {
                    Quality = Quality.Bad
                };

                return (objectStatus, noDataResult);
            }

            var statistics = _statCalcService.CalcEventsStat(events);

            var result = objectStatistics with
            {
                Statistics = statistics,
                Quality = Quality.Good
            };

            return (ObjectStatus.Ok, result);

        }
        catch (GeoDaDomainException ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return (objectStatus, objectStatistics);
        }
        catch (GeoDaAppException ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            var status = _geoDaDomainUtils.BuildObjectStatusByOpStatus(ex.OperationStatus);
            return (status, objectStatistics);
        }
    }

    public (ObjectStatus, ObjectStatistics) GetObjectStatistics(string objectName, DateTime dateTime)
    {
        var checkInterval = _settings.ObjectsSettings[objectName].StatCalcIntervalInDays;
        var result = GetObjectStatistics(objectName, dateTime, checkInterval);
        return result;
    }

    public (ObjectStatus, IReadOnlyList<Event>) GetEvents(string objectName, DateTime start, DateTime end, double minEnergy)
    {
        var objectStatus = ObjectStatus.Uncertain;

        try
        {
            (objectStatus, var objectId) = _objectAppService.GetObjectStatusAndId(objectName);

            if (objectStatus != ObjectStatus.Ok)
                return (objectStatus, Array.Empty<Event>());

            var events = _eventRepository.SelectEventsAtRangeGreaterOrEqualToEnergyLevel(objectId, start, end, minEnergy)
                .Where(x => x.IsSomeProperitiesIsNull() is false)
                .Select(x => x.ToEvent(_geoDaDomainUtils.ConvertAzonFormatToDateTime))
                .ToList();

            return (ObjectStatus.Ok, events);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return (objectStatus, Array.Empty<Event>());
        }
    }

    public (ObjectStatus, RegionalForecastBlocksInfo) GetCurrentRfBlocksInfo(string objectName)
    {
        try
        {
            var result = _rfActualDataStore.GetRfBlocksInfo(objectName);

            return result;
        }
        catch (GeoDaAppException ex)
        {
            return (ObjectStatus.Absent, _rfBlocksInfoFactory.CreateDefault());
        }        
    }

    public Dictionary<string, ObjectInfoAndStatus> GetObjectInfoAndStatuses() =>
        _objectAppService.GetObjectInfoAndStatuses(_settings.ObjectsSettings.Keys.ToList());

    public bool BuildDensityMapsImages(EnergyAssessment energyEssessment)
    {
        try
        {
            (var start, var end) = _geoDaDomainUtils.CreateDateInterval(energyEssessment.Object.CheckIntervalInDays,
                energyEssessment.Object.DateTimeOfAssessment);

            var events = _eventRepository.SelectEventsAtRange(energyEssessment.Object.ObjectId, start, end)
                .Where(x => x.IsSomeProperitiesIsNull() is false)
                .Select(x => x.ToEvent(_geoDaDomainUtils.ConvertAzonFormatToDateTime))
                .ToList();

            if (events.Count == 1 && events[0].Dt.TimeOfDay == new TimeSpan(0, 0, 0))
                events[0].Dt = events[0].Dt.AddSeconds(1);

            var isOk = _rfDensityMapImageService.CreateDensityMapsImages(energyEssessment.Object.ObjectId,
                energyEssessment.Object.DateTimeOfAssessment,
                _settings.ObjectsSettings[energyEssessment.Object.ObjectName],
                events);

            return isOk;

        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return false;
        }
    }

    public bool BuildBlocksInfoMapsImages(RegionalForecastBlocksInfo rfBlocksInfo)
    {
        try
        {
            if (rfBlocksInfo.DtOfInfoPreparation == DateTime.MinValue)
                return false;

            var paramFblocksInfoData = _rfParamFBlockDomainService.CreateParamFBlockInfo(rfBlocksInfo);
            var currentEnergyBlocksInfoData = _rfParamFBlockDomainService.CreateCurrentEnergyBlockInfo(rfBlocksInfo);

            var isOk = _rfBlocksInfoMapsService.CreateBlocksInfoMapsImages(rfBlocksInfo.ObjectId,
                rfBlocksInfo.DtOfInfoPreparation, _settings.ObjectsSettings[rfBlocksInfo.ObjectName],
                paramFblocksInfoData, currentEnergyBlocksInfoData);

            return isOk;

        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return false;
        }
    }

    public (bool IsOk, string fileName) PreparedFileWithEvents(
        string objectName,
        string pathForStore,
        DateTime start,
        DateTime end,
        double minEnergyValue)
    {
        try
        {
            (var status, var id) = _objectAppService.GetObjectStatusAndId(objectName);

            if (status == ObjectStatus.Ok)
            {
                // TODO: очищать где-нибудь в другом месте??
                ClearDataEventsDir(pathForStore);

                var events = _eventRepository.SelectEventsAtRange(id, start, end)
                .Where(x => x.IsSomeProperitiesIsNull() is false)
                .Where(x => x.E >= minEnergyValue)
                .Select(x => x.ToEvent(_geoDaDomainUtils.ConvertAzonFormatToDateTime))
                .OrderBy(x => x.Dt)
                .ToList();

                var eventRowToString = (Event x) => $"{x.Dt:dd.MM.yyyy}; {x.Dt:HH:mm:ss}; {x.X}; {x.Y}; {x.Z}; " +
                    $"{x.E.ToString("0.0000", CultureInfo.InvariantCulture)}";

                var stringForWrite = new List<string> { "D; T; X; Y; Z; E" };
                stringForWrite.AddRange(events.Select(x => eventRowToString(x)));

                var fileName = $"events_{objectName}_{start:dd.MM.yy}-{end:dd.MM.yy}.csv";

                var fullFileName = Path.Combine(pathForStore, fileName);

                File.WriteAllLines(fullFileName, stringForWrite);

                return (true, fileName);
            }
            else
            {
                return (false, string.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return (false, string.Empty);
        }
    }

    record GpnsCount(DateTime Dt, int CountOfGpns);

    public (bool IsOk, string fileName) PreparedFileWithGpnsCount(
        string objectName,
        string pathForStore,
        DateTime start,
        DateTime end)
    {
        try
        {
            (var status, var id) = _objectAppService.GetObjectStatusAndId(objectName);

            if (status == ObjectStatus.Ok)
            {
                // TODO: очищать где-нибудь в другом месте??
                ClearDataEventsDir(pathForStore);

                var gpns = _qualsgrRepository.SelectAllQualsgrsAtTimeRange(id, start, end)
                    .Select(x => new GpnsCount(
                        Dt: _geoDaDomainUtils.ConvertAzonFormatToDateTime(x.Idat, x.Itim),
                        CountOfGpns: x.Ngp ?? -1
                    ))
                    .OrderBy(x => x.Dt)
                    .ToList();
                                
                var gpnsRowToString = (GpnsCount x) => $"{x.Dt:dd.MM.yyyy}; {x.Dt:HH:mm:ss}; {x.CountOfGpns}";

                var stringForWrite = new List<string> { "D; T; Ngp" };
                stringForWrite.AddRange(gpns.Select(x => gpnsRowToString(x)));

                var fileName = $"gpns_{objectName}_{start:dd.MM.yy}-{end:dd.MM.yy}.csv";

                var fullFileName = Path.Combine(pathForStore, fileName);

                File.WriteAllLines(fullFileName, stringForWrite);

                return (true, fileName);
            }
            else
            {
                return (false, string.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return (false, string.Empty);
        }
    }

    public (bool IsOk, string fileName) PreparedFileWithBlocksInfo(string objectName, string pathForStore)
    {
        try
        {
            (var objectStatus, var rfBlocksInfo) = _rfActualDataStore.GetRfBlocksInfo(objectName);

            if (objectStatus != ObjectStatus.Ok)
                return (false, string.Empty);

            ClearDataEventsDir(pathForStore);

            var blockInfoToString = (BlockInfo x) => $"{x.BlockCenterCoords.X}; {x.BlockCenterCoords.Y}; " +
            $"{x.BlockCenterCoords.Z}; {x.F.ToString("0.0000", CultureInfo.InvariantCulture)}; {x.CountOfEvents}; " +
            $"{x.TotalEnergy.ToString("0.0000", CultureInfo.InvariantCulture)}; " +
            $"{x.MaxCurrentEnergy.ToString("0.0000", CultureInfo.InvariantCulture)}";

            var stringForWrite = new List<string> { "X; Y; Z; F; CountOfEvents; TotalEnergy; MaxCurrentEnergy" };

            List<BlockInfo> tmp = rfBlocksInfo.BlocksInfo.Cast<BlockInfo>().ToList();

            stringForWrite.AddRange(tmp.Select(x => blockInfoToString(x)));

            var fileName = $"blocksInfo_{objectName}_{_dateTimeService.GetCurrentDateTime():dd.MM.yy}.csv";

            var fullFileName = Path.Combine(pathForStore, fileName);

            File.WriteAllLines(fullFileName, stringForWrite);

            return (true, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return (false, string.Empty);
        }
    }

    public (bool IsOk, string fileName) PreparedFileWithArchBlocksInfo(string objectName, 
        string pathForStore, 
        BlockInfo[,,] blockInfos)
    {
        try
        {            
            ClearDataEventsDir(pathForStore);

            var blockInfoToString = (BlockInfo x) => $"{x.BlockCenterCoords.X}; {x.BlockCenterCoords.Y}; " +
            $"{x.BlockCenterCoords.Z}; {x.F.ToString("0.0000", CultureInfo.InvariantCulture)}; {x.CountOfEvents}; " +
            $"{x.TotalEnergy.ToString("0.0000", CultureInfo.InvariantCulture)}; " +
            $"{x.MaxCurrentEnergy.ToString("0.0000", CultureInfo.InvariantCulture)}";

            var stringForWrite = new List<string> { "X; Y; Z; F; CountOfEvents; TotalEnergy; MaxCurrentEnergy" };

            List<BlockInfo> tmp = blockInfos.Cast<BlockInfo>().ToList();

            stringForWrite.AddRange(tmp.Select(x => blockInfoToString(x)));

            var fileName = $"blocksInfo_{objectName}_{_dateTimeService.GetCurrentDateTime():dd.MM.yy}.csv";

            var fullFileName = Path.Combine(pathForStore, fileName);

            File.WriteAllLines(fullFileName, stringForWrite);

            return (true, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return (false, string.Empty);
        }
    }

    public (ObjectStatus, TimeSpan) GetWorkTime(string objectName, DateTime start, DateTime end)
    {
        var objectStatus = ObjectStatus.Uncertain;

        try
        {
            (objectStatus, var objectId) = _objectAppService.GetObjectStatusAndId(objectName);

            if (objectStatus != ObjectStatus.Ok)
                return (objectStatus, TimeSpan.Zero);

            var qs = _qualsgrRepository.SelectAllQualsgrsAtTimeRange(objectId, start, end)
                .Select(x => x.ToQualsgr())
                .ToList();

            if(qs.Count == 0)
                return (objectStatus, TimeSpan.Zero);

            var totalMinutes = 0;
            var prevIndex = GetFirstGoodQualsgr(qs);
            for (int i = prevIndex + 1; i < qs.Count; i++)
            {
                var dt = _geoDaDomainUtils.ConvertAzonFormatToDateTime(qs[i].Idat, qs[i].Itim);
                
                if (dt.Minute % 10 != 0)
                    continue;

                var dtPrev = _geoDaDomainUtils.ConvertAzonFormatToDateTime(qs[prevIndex].Idat, qs[prevIndex].Itim);

                var dtDiff = dt - dtPrev;
                if (dtDiff.TotalSeconds >= 590 && dtDiff.TotalSeconds <= 610)
                    totalMinutes += 10;

                prevIndex = i;
            }

            return (ObjectStatus.Ok, TimeSpan.FromMinutes(totalMinutes));

        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return (objectStatus, TimeSpan.Zero);
        }
    }

    private int GetFirstGoodQualsgr(IReadOnlyList<Qualsgr> qs)
    {
        if (qs.Count == 0)
            return -1;

        for(int i = 0; i < qs.Count; i++)
        {
            var dt = _geoDaDomainUtils.ConvertAzonFormatToDateTime(qs[i].Idat, qs[i].Itim);

            if (dt.Minute % 10 == 0)
                return i;
        }

        return -1;
    }

    public (ObjectStatus, IReadOnlyList<MsgLog>) GetMsgLogs(string objectName, DateTime start, DateTime end)
    {
        var objectStatus = ObjectStatus.Uncertain;

        try
        {
            (objectStatus, var objectId) = _objectAppService.GetObjectStatusAndId(objectName);

            if (objectStatus != ObjectStatus.Ok)
                return (objectStatus, new List<MsgLog>());

            var result = _msgLogRepository.SelectMessages(objectId, start, end)
                .Select(x => x.ToMsgLog(_geoDaDomainUtils.ConvertAzonFormatToDateTime))
                .ToList();

            return (ObjectStatus.Ok, result);

        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return (objectStatus, new List<MsgLog>());
        }        
    }

    public string GetMessageText(int messageCode, int errorCode, string additionText)
    {
        try
        {
            if (_msgCodeText.Count == 0)
                LoadMessageCodeText();

            if (_errorCodeText.Count == 0)
                LoadErrorCodeText();

            var msg = "Неизвестный тип сообщения.";
            if (_msgCodeText.ContainsKey(messageCode))
                msg = _msgCodeText[messageCode] + ".";

            var err = string.Empty;
            if (_errorCodeText.ContainsKey(errorCode))
                err = _errorCodeText[errorCode] + ". ";

            var result = $"{msg} {err} {additionText}";

            return result;
        }
        catch(Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);
            return $"Ошибка при интерпретации кода msg: {messageCode} и err: {errorCode}";
        }
    }

    public RegionalForecastObjectSettings? GetObjectSettings(string objectName) =>
        _settings.ObjectsSettings.TryGetValue(objectName, out var settings) ? settings : null;

    // Private
    private void LoadMessageCodeText()
    {
        try
        {
            _msgCodeText.Clear();

            _msgCodeTextRepository.SelectAll()
                .ToList()
                .ForEach(x => _msgCodeText.Add(x.Code, x.Text));
        }
        catch(Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);
        }
    }

    private void LoadErrorCodeText()
    {
        try
        {
            _errorCodeText.Clear();

            _errorCodeTextRepository.SelectAll()
                .ToList()
                .ForEach(x => _errorCodeText.Add(x.Code, x.Text));
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);
        }
    }

    private GpnsCountInfo GetGpnsCountInfoAtSpecificTime(int objectId, DateTime dateTime)
    {
        var startDt = dateTime.AddMinutes((-1) * MinutesForWatch);

        var qualsgrs = _qualsgrRepository.SelectAllQualsgrsAtTimeRange(objectId, startDt, dateTime)
            .Where(x => !x.IsSomeProperitiesIsNull())
            .Select(x => x.ToQualsgr())
            .ToList();

        if (qualsgrs == null || qualsgrs.Count == 0)
        {
            _logger.LogWarning($"RF :: ObjectId: {objectId}: нет данных для определения кол-ва геофонов");

            var noDataResult = new GpnsCountInfo(Quality.Bad, 0, DateTime.MinValue);

            return noDataResult;
        }

        var newestQualsgr = _qualsgrService.GetNewestQualsgr(qualsgrs);

        var gpnsCountDateTime = _geoDaDomainUtils.ConvertAzonFormatToDateTime(newestQualsgr.Idat, newestQualsgr.Itim);

        var result = new GpnsCountInfo(Quality.Good, newestQualsgr.Ngp, gpnsCountDateTime);

        return result;
    }

    private static ObjectStatus BuildObjectStatusByOpStatus(OpStatus opStatus) =>
        opStatus switch
        {
            OpStatus.GetDataError => ObjectStatus.DbError,
            OpStatus.InsertDataError => ObjectStatus.DbError,
            _ => ObjectStatus.Uncertain
        };

    private void ClearDataEventsDir(string path)
    {
        var dirInfo = new DirectoryInfo(path);

        foreach (var f in dirInfo.GetFiles())
        {
            try
            {
                if (_dateTimeService.GetCurrentDateTime().AddMinutes((-1) * 10) > f.CreationTime)
                    f.Delete();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: ex.Message);
            }
        }
    }
}
