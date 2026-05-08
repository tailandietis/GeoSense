using GeoDa.Domain.Models;

namespace GeoDa.BlazorWebApp.Services.WebAppUtils;

internal interface IWebAppUtils
{
    string BuildLabel(
        ObjectStatus objectStatus,
        Quality quality,
        string prefix,
        string goodData,
        string badData = "Нет данных!");
}
