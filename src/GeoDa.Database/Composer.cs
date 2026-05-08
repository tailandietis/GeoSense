using AlarmMonitor.Brokers.Storages.AlarmMonitorStorageBrokers;
using GeoDa.Application.AlarmMonitors.Repository.ActiveAlarmsCaches;
using GeoDa.Application.Authentication.Repository.LoginLoggers;
using GeoDa.Application.Databases;
using GeoDa.Application.GeneralForecasts.Repository.ErrorCodeTexts;
using GeoDa.Application.GeneralForecasts.Repository.Geometries;
using GeoDa.Application.GeneralForecasts.Repository.Geophones;
using GeoDa.Application.GeneralForecasts.Repository.GsParams;
using GeoDa.Application.GeneralForecasts.Repository.MsgCodeTexts;
using GeoDa.Application.GeneralForecasts.Repository.MsgLogs;
using GeoDa.Application.GeneralForecasts.Repository.ObjectInfos;
using GeoDa.Application.GeneralForecasts.Repository.Qualgps;
using GeoDa.Application.GeneralForecasts.Repository.Qualsgrs;
using GeoDa.Application.RegionalForecasts.Repository.Events;
using GeoDa.Application.RegionalForecasts.Repository.GsAlarms;
using GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies;
using GeoDa.Application.RegionalForecasts.Repository.GsStats;
using GeoDa.Application.RegionalForecasts.Repository.RegionalForecastActualDataStores;
using GeoDa.Database.InMemory.AlarmMonitors;
using GeoDa.Database.InMemory.RegionalForecasts;
using GeoDa.Database.Postgres;
using GeoDa.Database.Postgres.Authentication;
using GeoDa.Database.Postgres.GeneralForecasts;
using GeoDa.Database.Postgres.RegionalForecasts;
using GeodaWebAppEngine.Brokers.SettingsBrokers;
using Microsoft.Extensions.DependencyInjection;

namespace GeoDa.Database;

public static class Composer
{
    public static IServiceCollection AddDatabases(this IServiceCollection services)
    {
        services.AddSingleton<IDbUtils, DbUtils>();

        services.AddTransient<IGeometryRepository, GeometryRepository>();
        services.AddTransient<IGeophoneRepository, GeophoneRepository>();
        services.AddTransient<IGsParamRepository, GsParamRepository>();
        services.AddTransient<IObjectInfoRepository, ObjectInfoRepository>();
        services.AddTransient<IQualgpRepository, QualgpRepository>();
        services.AddTransient<IQualsgrRepository, QualsgrRepository>();
        services.AddTransient<IMsgLogRepository, MsgLogRepository>();
        services.AddTransient<IMsgCodeTextRepository, MsgCodeTextRepository>();
        services.AddTransient<IErrorCodeTextRepository, ErrorCodeTextRepository>();

        services.AddTransient<IEventRepository, EventRepository>();
        services.AddTransient<IGsAlarmRepository, GsAlarmRepository>();
        services.AddTransient<IGsStatRepository, GsStatRepository>();
        services.AddTransient<IGsParamFEnergyRepository, GsParamFEnergyRepository>();

        services.AddSingleton<IRegionalForecastActualDataStore, RegionalForecastActualDataStore>();

        services.AddSingleton<IAlarmCodeDescriptionRepository, AlarmCodeDescriptionRepository>();
        services.AddSingleton<IAlarmItemRepository, AlarmItemRepository>();
        services.AddSingleton<IActiveAlarmsStore, ActiveAlarmsStore>();

        services.AddSingleton<ILoginLoggerRepository, LoginLoggerRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();

        return services;
    }
}
