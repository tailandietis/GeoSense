using GeoDa.Domain.AlarmMonitors.Services;
using GeoDa.Domain.Combines.Services;
using GeoDa.Domain.CurrentForecasts.Services.DfmCubes;
using GeoDa.Domain.CurrentForecasts.Services.MassifStates;
using GeoDa.Domain.CurrentForecasts.Services.SliceImages;
using GeoDa.Domain.CurrentForecasts.Services.SystemQualities;
using GeoDa.Domain.DigitalRepers.Services;
using GeoDa.Domain.GeneralForecasts.Services.Objects;
using GeoDa.Domain.GeneralForecasts.Services.Qualsgrs;
using GeoDa.Domain.RegionalForecasts.Services.BlocksInfoMaps;
using GeoDa.Domain.RegionalForecasts.Services.EnergyAssessments;
using GeoDa.Domain.RegionalForecasts.Services.Factories;
using GeoDa.Domain.RegionalForecasts.Services.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Services.StatCalcs;
using GeoDa.Domain.Services.GeoDaUtils;
using GeoDa.Domain.SupportPosts.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GeoDa.Domain;

public static class Composer
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddSingleton<IGeoDaDomainUtils, GeoDaDomainUtils>();
        services.AddTransient<IObjectService, ObjectService>();
        services.AddTransient<IQualsgrService, QualsgrService>();

        // GeneralForecast
        services.AddSingleton<IObjectInfoFactory, ObjectInfoFactory>();

        // Alarm
        services.AddSingleton<IAlarmCodeFactory, AlarmCodeFactory>();
        services.AddSingleton<IAlarmIdFactory, AlarmIdFactory>();

        // Regional Forecast
        services.AddSingleton<IEnergyAssessmentFactory, EnergyAssessmentFactory>();
        services.AddSingleton<IEnergyReportObjectInfoFactory, EnergyReportObjectInfoFactory>();
        services.AddSingleton<IEnergyReportFactory, EnergyReportFactory>();
        services.AddSingleton<IEventsStatisticsFactory, EventsStatisticsFactory>();
        services.AddSingleton<IObjectStatisticsFactory, ObjectStatisticsFactory>();
        services.AddSingleton<IRegionalForecastBlocksInfoFactory, RegionalForecastBlocksInfoFactory>();

        services.AddTransient<IStatCalcService, StatCalcService>();
        services.AddTransient<IEnergyAssessmentService, EnergyAssessmentService>();
        services.AddTransient<IRfDensityMapsDomainService, RfDensityMapsDomainService>();
        services.AddTransient<IRfBlocksInfoMapsDomainService, RfBlocksInfoMapsDomainService>();
        services.AddTransient<IRfParamFBlockDomainService, RfParamFBlockDomainService>();

        // Current Forecast
        services.AddSingleton<ISystemQualityFactory, SystemQualityFactory>();
        services.AddSingleton<IMassifStateFactory, MassifStateFactory>();

        services.AddTransient<IDfmCubeService, DfmCubeService>();
        services.AddTransient<IMassifStateService, MassifStateService>();
        services.AddTransient<ISliceImageDomainService, SliceImageDomainService>();

        // Support Post
        services.AddSingleton<ISupportPostDataFactory, SupportPostDataFactory>();
        services.AddSingleton<ISupportPostParameterFactory, SupportPostParameterFactory>();

        services.AddTransient<ISupportPostDomainService, SupportPostDomainService>();

        // Combine
        services.AddSingleton<ICombineDataFactory, CombineDataFactory>();
        services.AddSingleton<ICombineParameterFactory, CombineParameterFactory>();

        services.AddTransient<ICombineDomainService, CombineDomainService>();

        // DigitalReper
        services.AddSingleton<IDigitalReperDataFactory, DigitalReperDataFactory>();
        services.AddSingleton<IDigitalReperParameterFactory, DigitalReperParameterFactory>();

        services.AddTransient<IDigitalReperDomainService, DigitalReperDomainService>();

        return services;
    }
}
