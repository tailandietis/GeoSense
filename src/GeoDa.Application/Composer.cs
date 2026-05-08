using GeoDa.Application.AlarmMonitors.Services.AlarmMonitorServices;
using GeoDa.Application.Authentication.Services;
using GeoDa.Application.GeneralForecasts.Services.Utils;
using GeoDa.Application.Objects;
using GeoDa.Application.RegionalForecasts.HostedServices;
using GeoDa.Application.RegionalForecasts.Services;
using GeoDa.Application.RegionalForecasts.Services.DensityMaps;
using GeoDa.Application.RegionalForecasts.Services.ParamFMaps;
using GeoDa.Application.RegionalForecasts.Services.RfBlocksInfoMaps;
using GeoDa.Application.Settings.HostedServices;
using GeoDa.Application.Settings.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GeoDa.Application
{
    public static class Composer
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<IGeneralForecastUtilsService, GeneralForecastUtilsService>();
            services.AddSingleton<IAlarmMonitorService, AlarmMonitorService>();
            services.AddSingleton<ILoginLoggerService, LoginLoggerService>();
            services.AddSingleton<IObjectAppService, ObjectAppService>();

            services.AddHostedService<SettingsHostedService>();
            services.AddSingleton<ISettingsService, SettingsService>();

            services.AddSingleton<IRegionalForecastService, RegionalForecastService>();
            services.AddHostedService<RegionalForecastHostedService>();
            services.AddTransient<IRfDensityMapsService, RfDensityMapsService>();
            services.AddTransient<IRfBlocksInfoMapsService, RfBlocksInfoMapsService>();

            return services;
        }
    }
}
