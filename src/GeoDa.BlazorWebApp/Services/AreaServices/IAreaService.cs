using GeoDa.BlazorWebApp.Models.Area.AreaConfigurations;

namespace GeoDa.BlazorWebApp.Services.AreaServices;

internal interface IAreaService
{
    AreaConfiguration GetAreaConfiguration();
}
