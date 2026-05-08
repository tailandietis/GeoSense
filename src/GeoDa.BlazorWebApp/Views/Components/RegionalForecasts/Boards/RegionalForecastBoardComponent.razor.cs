using GeoDa.BlazorWebApp.Models.Area.BoardConfigurations;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace GeoDa.BlazorWebApp.Views.Components.RegionalForecasts.Boards;

public partial class RegionalForecastBoardComponent
{
    [Parameter]
    public BoardConfiguration BoardConfiguration { get; set; } = new();

    public Dictionary<string, RegionalForecastObjectBoardComponent> RfObjects = new();

    // Private
    private string GetTitle() =>
        BoardConfiguration.Title;
}
