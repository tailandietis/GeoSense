using GeoDa.Infrastructure.Services.DateTimes;
using GeoDa.Infrastructure.Services.JsonFiles;
using Microsoft.Extensions.DependencyInjection;

namespace GeoDa.Infrastructure;

public static class Composer
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IJsonFileService, JsonFileService>();
        return services;
    }
}
