using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;

namespace GeoDa.BlazorWebApp.Services.WebAppUtils;

internal class WebAppUtils : IWebAppUtils
{
    private readonly ILogger<WebAppUtils> _logger;

    public WebAppUtils(ILogger<WebAppUtils> logger)
    {
        _logger = logger;
    }

    public string BuildLabel(
            ObjectStatus objectStatus,
            Quality quality,
            string prefix,
            string goodData,
            string badData = "Нет данных!") =>
           prefix + objectStatus switch
           {
               ObjectStatus.Ok => quality switch
               {
                   Quality.Good => goodData,
                   Quality.Bad => badData,
                   Quality.Uncertain => "PS: Uncertain",
                   _ => "PS: ERROR!"
               },
               ObjectStatus.DbError => "Ошибка подключения",
               ObjectStatus.Absent => "Нет объекта",
               ObjectStatus.HasDuplicate => "Дублирование объектов в базе",
               ObjectStatus.NotConnected => "Ошибка связи",
               ObjectStatus.Uncertain => "ObS: Uncertain",
               _ => "ObS: ERROR!"
           };
}
