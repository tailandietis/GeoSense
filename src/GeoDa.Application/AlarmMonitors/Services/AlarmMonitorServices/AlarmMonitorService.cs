using AlarmMonitor.Brokers.Storages.AlarmMonitorStorageBrokers;
using GeoDa.Application.AlarmMonitors.Extensions;
using GeoDa.Application.AlarmMonitors.Models;
using GeoDa.Application.AlarmMonitors.Models.Factories;
using GeoDa.Application.AlarmMonitors.Repository.ActiveAlarmsCaches;
using GeoDa.Application.AlarmMonitors.Repository.AlarmItems.Dtos;
using GeoDa.Application.GeneralForecasts.Extensions;
using GeoDa.Application.GeneralForecasts.Repository.ObjectInfos;
using GeoDa.Application.GeneralForecasts.Services.Utils;
using GeoDa.Domain.AlarmMonitors.Models;
using GeoDa.Domain.AlarmMonitors.Services;
using GeoDa.Domain.CurrentForecasts;
using GeoDa.Domain.CurrentForecasts.Models.Settings;
using GeoDa.Domain.GeneralForecasts.Services.Objects;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using GeoDa.Infrastructure.Services.DateTimes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Application.AlarmMonitors.Services.AlarmMonitorServices;

internal class AlarmMonitorService : IAlarmMonitorService
{
    private readonly string _alarmDbName = "DbGeoDA";
    
    private readonly IObjectInfoRepository _objectInfoRepository;
    private readonly IAlarmCodeDescriptionRepository _alarmCodeDescriptionRepository;
    private readonly IAlarmItemRepository _alarmItemRepository;

    private readonly IObjectService _objectsService;
    private readonly IDateTimeService _dateTimeService;

    private readonly IActiveAlarmsStore _activeAlarmsStore;

    private readonly IGeneralForecastUtilsService _gfUtils;

    private readonly IAlarmCodeFactory _alarmCodeFactory;
    private readonly IAlarmIdFactory _alarmIdFactory;

    private readonly RegionalForecastSettings _rfSettings;
    private readonly CurrentForecastSettings _cfSettings;

    private readonly ILogger<AlarmMonitorService> _logger;

    private readonly Dictionary<AlarmCode, string> _alarmCodeDescriptions;

    private readonly string _dbConnStringRf = string.Empty;
    private readonly string _dbConnStringCf = string.Empty;

    private readonly object _locker = new object();

    public AlarmMonitorService(
        IObjectInfoRepository objectInfoRepository,
        IAlarmCodeDescriptionRepository alarmCodeDescriptionRepository,
        IAlarmItemRepository alarmItemRepository,
        IObjectService objectsService,
        IDateTimeService dateTimeService,
        IActiveAlarmsStore activeAlarmsStore,
        IGeneralForecastUtilsService rfUtils,
        IAlarmCodeFactory alarmCodeFactory,
        IAlarmIdFactory alarmIdFactory,
        IOptions<RegionalForecastSettings> rfSettings,
        IOptions<CurrentForecastSettings> cfSettings,
        ILogger<AlarmMonitorService> logger)
    {
        _objectInfoRepository = objectInfoRepository;
        _alarmCodeDescriptionRepository = alarmCodeDescriptionRepository;
        _alarmItemRepository = alarmItemRepository;

        _objectsService = objectsService;
        _dateTimeService = dateTimeService;

        _activeAlarmsStore = activeAlarmsStore;

        _gfUtils = rfUtils;

        _alarmCodeFactory = alarmCodeFactory;
        _alarmIdFactory = alarmIdFactory;

        _rfSettings = rfSettings.Value;
        _cfSettings = cfSettings.Value;

        _logger = logger;

        _alarmCodeDescriptions = new();

        _dbConnStringRf = _gfUtils.BuildDbConnectionString(_rfSettings.DbSettings.DbName);
        _dbConnStringCf = _gfUtils.BuildDbConnectionString(_cfSettings.DbSettings.DbName);

        var dbConnStringAlarm = _gfUtils.BuildDbConnectionString(_alarmDbName);
        _alarmCodeDescriptionRepository.DbConnectionString = dbConnStringAlarm;
        _alarmItemRepository.DbConnectionString = dbConnStringAlarm;

        UpdateAlarmCodeDescriptions();
    }

    // IAlarmMonitorService
    public AlarmInfo BuildActiveAlarmInfo(string objectName, AlarmId alarmId)
    {
        var result = AlarmInfoFactory.Create(alarmId: alarmId,
            objectName: objectName,
            dateTime: _dateTimeService.GetCurrentDateTime(),
            alarmStatus: AlarmStatus.Active);

        return result;
    }

    public void AddAlarm(AlarmInfo alarmInfo)
    {
        var isExists = _activeAlarmsStore.IsAlarmExists(alarmInfo.AlarmId);

        if (isExists is false)
        {
            _activeAlarmsStore.AddAlarm(alarmInfo);
            _alarmItemRepository.InsertAlarmItem(alarmInfo.ToAlarmItemDto());
        }
    }

    public bool IsAlarmExists(AlarmId alarmId) =>
        _activeAlarmsStore.IsAlarmExists(alarmId);

    public bool IsAnyAlarmOccured() =>
        _activeAlarmsStore.IsAnyAlarm();

    public void RemoveActiveAlarm(AlarmId alarmId)
    {
        var isExists = _activeAlarmsStore.IsAlarmExists(alarmId);

        if (isExists)
        {
            _activeAlarmsStore.RemoveAlarm(alarmId);

            var alarm = new AlarmItemDto()
            {
                ModuleFamilyCode = alarmId.AlarmCode.ModuleFamilyCode,
                ObjectId = alarmId.ObjectId,
                ServiceFamilyCode = alarmId.AlarmCode.ServiceFamilyCode,
                ErrorCode = alarmId.AlarmCode.ErrorCode,
                Status = (int)AlarmStatus.Deactived,
                Dt = _dateTimeService.GetCurrentDateTime()
            };

            _alarmItemRepository.InsertAlarmItem(alarm);
        }
    }

    public void SetAlarmStatus(AlarmId alarmId, AlarmStatus alarmStatus)
    {
        _activeAlarmsStore.UpdateAlarmStatus(alarmId, alarmStatus);

        var alarm = new AlarmItemDto()
        {
            ModuleFamilyCode = alarmId.AlarmCode.ModuleFamilyCode,
            ObjectId = alarmId.ObjectId,
            ServiceFamilyCode = alarmId.AlarmCode.ServiceFamilyCode,
            ErrorCode = alarmId.AlarmCode.ErrorCode,
            Status = (int)alarmStatus,
            Dt = _dateTimeService.GetCurrentDateTime()
        };

        _alarmItemRepository.InsertAlarmItem(alarm);
    }

    public IReadOnlyList<AlarmInfo> GetAllActiveAlarms() =>
        _activeAlarmsStore.GetAllAlarms();

    public string GetTextDescription(AlarmId alarmId)
    {
        lock (_locker)
        {
            if (_alarmCodeDescriptions.Count == 0)
                UpdateAlarmCodeDescriptions();

            if (_alarmCodeDescriptions.ContainsKey(alarmId.AlarmCode) is false)
                return "Неизвестный код ошибки";

            var objName = string.Empty;
            if (_activeAlarmsStore.IsAlarmExists(alarmId))
            {
                objName = _activeAlarmsStore.GetAlarm(alarmId).ObjectName;
            }
            else
            {
                (var status, var text) = GetObjectName(alarmId.AlarmCode.ModuleFamilyCode, alarmId.ObjectId);
                if (status == OpStatus.Ok)
                    objName = text;
                else
                    return text;
            }

            var description = _alarmCodeDescriptions[alarmId.AlarmCode];

            return $"Объект: {objName}. " + description;
        }
    }

    public IReadOnlyList<AlarmInfo> GetAlarmsFromArchive(
        int moduleFamilyCode,
        int objectId,
        DateTime start,
        DateTime end)
    {
        (var status, var objectName) = GetObjectName(moduleFamilyCode, objectId);

        if (status != OpStatus.Ok)
            return new List<AlarmInfo>().AsReadOnly();

        if (moduleFamilyCode == RegionalForecastConsts.ModuleFamilyCode
            || moduleFamilyCode == CurrentForecastConsts.ModuleFamilyCode)
        {
            var result = _alarmItemRepository.SelectAlarmItems(moduleFamilyCode, objectId, start, end)
                .Select(x => x.ToAlarmInfo(objectName, _alarmCodeFactory, _alarmIdFactory))
                .ToList()
                .AsReadOnly();

            return result;
        }
        

        return new List<AlarmInfo>().AsReadOnly();
    }


    // Private
    private (OpStatus, string) GetObjectName(int moduleFamilyCode, int objectId)
    {
        try
        {
            if (moduleFamilyCode == RegionalForecastConsts.ModuleFamilyCode)
            {
                _objectInfoRepository.DbConnectionString = _dbConnStringRf;
                var objects = _objectInfoRepository.SelectAllObjectInfos()
                    .Select(x => x.ToObjectInfo())
                    .ToList();

                var objectName = _objectsService.GetObjectNameById(objectId, objects);

                return (OpStatus.Ok, objectName);
            }
            else if (moduleFamilyCode == CurrentForecastConsts.ModuleFamilyCode)
            {
                _objectInfoRepository.DbConnectionString = _dbConnStringCf;
                var objects = _objectInfoRepository.SelectAllObjectInfos()
                    .Select(x => x.ToObjectInfo())
                    .ToList();

                var objectName = _objectsService.GetObjectNameById(objectId, objects);

                return (OpStatus.Ok, objectName);
            }

            return (OpStatus.UnkonwnError, $"Неизвестный ModuleFamilyCode: {moduleFamilyCode}");
        }
        catch
        {
            return (OpStatus.UnkonwnError, "Ошибка получания имени объекта по его Id");
        }
    }


    private void UpdateAlarmCodeDescriptions()
    {
        lock (_locker)
        {
            try
            {
                var data = _alarmCodeDescriptionRepository.SelectAllAlarmCodeDescriptions();

                _alarmCodeDescriptions.Clear();

                foreach (var code in data)
                {
                    var alarmCode = _alarmCodeFactory.Create(code.ModuleFamilyCode, code.ServiceFamilyCode,
                        code.ErrorCode);

                    _alarmCodeDescriptions.Add(alarmCode, code.Msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: ex.Message);

                _alarmCodeDescriptions.Clear();
            }
        }
    }
}
