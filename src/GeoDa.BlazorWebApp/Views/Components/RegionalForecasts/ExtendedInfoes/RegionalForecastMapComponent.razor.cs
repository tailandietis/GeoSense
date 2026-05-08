using Microsoft.AspNetCore.Components;

namespace GeoDa.BlazorWebApp.Views.Components.RegionalForecasts.ExtendedInfoes;

public partial class RegionalForecastMapComponent
{
    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string ImagePath { get; set; } = string.Empty;
}
