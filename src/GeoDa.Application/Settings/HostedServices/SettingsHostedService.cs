using GeoDa.Application.GeneralForecasts.Repository.GsParams;
using GeoDa.Application.GeneralForecasts.Repository.GsParams.Dtos;
using GeoDa.Application.GeneralForecasts.Services.Utils;
using GeoDa.Domain.CurrentForecasts.Models.Settings;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using GeoDa.Infrastructure.Services.DateTimes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;

namespace GeoDa.Application.Settings.HostedServices
{
    internal class SettingsHostedService : BackgroundService
    {
        private readonly IGsParamRepository _gsParamsRepository;

        private readonly IDateTimeService _dateTimeService;
        private readonly IGeneralForecastUtilsService _rfUtilsService;

        private readonly RegionalForecastSettings _rfSettings;
        private readonly CurrentForecastSettings _cfSettings;

        private readonly ILogger<SettingsHostedService> _logger;


        public SettingsHostedService(
            IGsParamRepository gsParamsRepository,
            IDateTimeService dateTimeService,
            IGeneralForecastUtilsService rfUtilsService,
            IOptions<RegionalForecastSettings> rfSettings,
            IOptions<CurrentForecastSettings> cfSettings,
            ILogger<SettingsHostedService> logger)
        {
            _gsParamsRepository = gsParamsRepository;

            _dateTimeService = dateTimeService;
            _rfUtilsService = rfUtilsService;

            _rfSettings = rfSettings.Value;
            _cfSettings = cfSettings.Value;

            _logger = logger;            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            var dt = _dateTimeService.GetCurrentDateTime();

            var rfParsToSave = GetRegionalForecastSettings(dt);

            var dbConnString = _rfUtilsService.BuildDbConnectionString(_rfSettings.DbSettings.DbName);
            _gsParamsRepository.DbConnectionString = dbConnString;
            _gsParamsRepository.InsertGsParams(rfParsToSave);

            var cfParsToSave = GetCurrentForecastSettings(dt);

            dbConnString = _rfUtilsService.BuildDbConnectionString(_cfSettings.DbSettings.DbName);
            _gsParamsRepository.DbConnectionString = dbConnString;
            _gsParamsRepository.InsertGsParams(cfParsToSave);

            await Task.CompletedTask;
        }

        private List<GsParamDto> GetRegionalForecastSettings(DateTime dt)
        {
            var pars = new List<GsParamDto>();

            var rfGeneralSettings = GetRegionalForecastGeneralSettings(dt);
            pars.AddRange(rfGeneralSettings);

            foreach (var objectName in _rfSettings.ObjectsSettings.Keys)
            {
                var tmp = GetRegionalForecastObjectSettings(objectName, dt);
                pars.AddRange(tmp);
            }

            return pars;
        }

        private List<GsParamDto> GetRegionalForecastGeneralSettings(DateTime dateTime)
        {
            var pars = new List<GsParamDto>
            {
                new()
                {
                    ParName = "AlarmCheckUpdatePauseInSeconds",
                    ParVal = _rfSettings.GeneralSettings.AlarmCheckUpdatePauseInSeconds.ToString()
                },
                new()
                {
                    ParName = "StatCalcUpdatePauseInSeconds",
                    ParVal = _rfSettings.GeneralSettings.StatCalcUpdatePauseInSeconds.ToString()
                },
                new()
                {
                    ParName = "ParamFCalcUpdatePauseInSeconds",
                    ParVal = _rfSettings.GeneralSettings.ParamFCalcUpdatePauseInSeconds.ToString()
                }
            };

            for (int i = 0; i < pars.Count; i++)
            {
                pars[i].Dt = dateTime;
                pars[i].ObjName = string.Empty;
            }

            return pars;
        }

        private List<GsParamDto> GetRegionalForecastObjectSettings(string objectName, DateTime dateTime)
        {
            var pars = new List<GsParamDto>()
            {
                new()
                {
                    ParName = nameof(RegionalForecastObjectSettings.AlarmELevel),
                    ParVal = _rfSettings.ObjectsSettings[objectName].AlarmELevel.ToString()
                },
                new()
                {
                    ParName = nameof(RegionalForecastObjectSettings.AlarmCheckIntervalInDays),
                    ParVal = _rfSettings.ObjectsSettings[objectName].AlarmCheckIntervalInDays.ToString()
                },
                new()
                {
                    ParName = nameof(RegionalForecastObjectSettings.StatCalcIntervalInDays),
                    ParVal = _rfSettings.ObjectsSettings[objectName].StatCalcIntervalInDays.ToString()
                },
                new()
                {
                    ParName = nameof(RegionalForecastObjectSettings.CountOfBlockAtWidth),
                    ParVal = _rfSettings.ObjectsSettings[objectName].CountOfBlockAtWidth.ToString()
                },
                new()
                {
                    ParName = nameof(RegionalForecastObjectSettings.CountOfBlockAtHeight),
                    ParVal = _rfSettings.ObjectsSettings[objectName].CountOfBlockAtHeight.ToString()
                },
                new()
                {
                    ParName = nameof(RegionalForecastObjectSettings.LayoutFileName),
                    ParVal = _rfSettings.ObjectsSettings[objectName].LayoutFileName.ToString()
                },
                new()
                {
                    ParName = "FieldCoords.X",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.FieldSettings.FieldCoords.X.ToString()
                },
                new()
                {
                    ParName = "FieldCoords.Y",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.FieldSettings.FieldCoords.Y.ToString()
                },
                new()
                {
                    ParName = "FieldCoords.Z",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.FieldSettings.FieldCoords.Z.ToString()
                },
                new()
                {
                    ParName = "FieldSize.X",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.FieldSettings.FieldSize.X.ToString()
                },
                new()
                {
                    ParName = "FieldSize.Y",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.FieldSettings.FieldSize.Y.ToString()
                },
                new()
                {
                    ParName = "FieldSize.Z",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.FieldSettings.FieldSize.Z.ToString()
                },
                new()
                {
                    ParName = "BlockSize",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.ParamFCalcSettings.BlockSize.ToString()
                },
                new()
                {
                    ParName = "TimeSpanMinImpactInDays",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.ParamFCalcSettings.TimeSpanMinImpactInDays.ToString()
                },
                new()
                {
                    ParName = "TimeSpanRegInDays",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.ParamFCalcSettings.TimeSpanRegInDays.ToString()
                },
                new()
                {
                    ParName = "BackgroundEnergyValue",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.ParamFCalcSettings.BackgroundEnergyValue.ToString()
                },
                new()
                {
                    ParName = "RadiusAddition",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.RadiusAddition.ToString()
                },
                new()
                {
                    ParName = "FBackgound",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.EstimationSettings.FBackgound.ToString()
                },
                new()
                {
                    ParName = "ECurrentBackground",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.EstimationSettings.ECurrentBackground.ToString()
                },
                new()
                {
                    ParName = "FLevel1",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.EstimationSettings.FLevel1.ToString()
                },
                new()
                {
                    ParName = "ECurrentLevel1",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.EstimationSettings.ECurrentLevel1.ToString()
                },
                new()
                {
                    ParName = "FLevel2",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.EstimationSettings.FLevel2.ToString()
                },
                new()
                {
                    ParName = "ECurrentLevel2",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.EstimationSettings.ECurrentLevel2.ToString()
                },
                new()
                {
                    ParName = "FLevel3",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.EstimationSettings.FLevel3.ToString()
                },
                new()
                {
                    ParName = "ECurrentLevel3",
                    ParVal = _rfSettings.ObjectsSettings[objectName].ParamFBlockSettings.EstimationSettings.ECurrentLevel3.ToString()
                },
            };

            for(int i = 0; i < pars.Count; i++)
            {
                pars[i].Dt = dateTime;
                pars[i].ObjName = objectName;
            }
            
            return pars;
        }

        private List<GsParamDto> GetCurrentForecastSettings(DateTime dt)
        {
            var pars = new List<GsParamDto>();

            var cfGeneralSettings = GetCurrentForecastGeneralSettings(dt);
            pars.AddRange(cfGeneralSettings);

            foreach (var objectName in _cfSettings.ObjectsSettings.Keys)
            {
                var tmp = GetCurrentForecastObjectSettings(objectName, dt);
                pars.AddRange(tmp);
            }

            return pars;
        }

        private List<GsParamDto> GetCurrentForecastGeneralSettings(DateTime dateTime)
        {
            var pars = new List<GsParamDto>
            {
                new()
                {
                    Dt = dateTime,
                    ObjName = string.Empty,
                    ParName = "FeaturesUpdatePauseInSeconds",
                    ParVal = _cfSettings.GeneralSettings.FeaturesUpdatePauseInSeconds.ToString()
                }
            };

            return pars;
        }

        private List<GsParamDto> GetCurrentForecastObjectSettings(string objectName, DateTime dateTime)
        {
            var pars = new List<GsParamDto>()
            {
                new()
                {
                    Dt = dateTime,
                    ObjName = objectName,
                    ParName = "DSliceLL",
                    ParVal = _cfSettings.ObjectsSettings[objectName].DSliceLL.ToString()
                },
                new()
                {
                    Dt = dateTime,
                    ObjName = objectName,
                    ParName = "DSliceHH",
                    ParVal = _cfSettings.ObjectsSettings[objectName].DSliceHH.ToString()
                },
                new()
                {
                    Dt = dateTime,
                    ObjName = objectName,
                    ParName = "DBlockLL",
                    ParVal = _cfSettings.ObjectsSettings[objectName].DBlockLL.ToString()
                },
                new()
                {
                    Dt = dateTime,
                    ObjName = objectName,
                    ParName = "BlockX0",
                    ParVal = _cfSettings.ObjectsSettings[objectName].BlockX0.ToString()
                },
                new()
                {
                    Dt = dateTime,
                    ObjName = objectName,
                    ParName = "BlockX1",
                    ParVal = _cfSettings.ObjectsSettings[objectName].BlockX1.ToString()
                },
                new()
                {
                    Dt = dateTime,
                    ObjName = objectName,
                    ParName = "BlockY0",
                    ParVal = _cfSettings.ObjectsSettings[objectName].BlockY0.ToString()
                },
                new()
                {
                    Dt = dateTime,
                    ObjName = objectName,
                    ParName = "BlockY1",
                    ParVal = _cfSettings.ObjectsSettings[objectName].BlockY1.ToString()
                },
                new()
                {
                    Dt = dateTime,
                    ObjName = objectName,
                    ParName = "MaxDValue",
                    ParVal = _cfSettings.ObjectsSettings[objectName].MaxDValue.ToString()
                },
            };

            return pars;
        }
    }
}
