using GeoDa.BlazorWebApp.Models.Area.AreaConfigurations;
using GeoDa.BlazorWebApp.Models.Area.BoardConfigurations;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.BlazorWebApp.Services.AreaServices;

internal class AreaService : IAreaService
{
    private const string RegionalForecastTitle = "Региональный прогноз";

    private readonly RegionalForecastSettings _rfSettings;
    private readonly ILogger<AreaService> _logger;

    public AreaService(
        IOptions<RegionalForecastSettings> rfSettings,
        ILogger<AreaService> logger)
    {
        _logger = logger;
        _rfSettings = rfSettings.Value;
    }

    public AreaConfiguration GetAreaConfiguration()
    {
        var boards = new List<BoardConfiguration>
        {
            new BoardConfiguration
            {
                BoardType = BoardType.RegionalForecast,
                Title = RegionalForecastTitle,
                ObjectNames = _rfSettings.ObjectsSettings.Keys.ToList()
            }
        };

        return new AreaConfiguration { Boards = boards };
    }
}
