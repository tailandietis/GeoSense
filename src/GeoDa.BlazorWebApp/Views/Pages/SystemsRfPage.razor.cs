using GeoDa.BlazorWebApp.Models.Area.BoardConfigurations;
using GeoDa.BlazorWebApp.Services.AreaServices;
using GeoDa.BlazorWebApp.Services.Observers;
using GeoDa.BlazorWebApp.Views.Components.RegionalForecasts.Boards;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class SystemsRfPage : ComponentBase, IObserverClient, IDisposable
{
    [Inject] private IAreaService AreaService { get; set; } = default!;
    [Inject] private IObserverService ObserverService { get; set; } = default!;

    private BoardConfiguration? _boardConfiguration;
    private RegionalForecastBoardComponent _rfBoard = new();

    protected override void OnInitialized()
    {
        if (!ObserverService.IsRegistered(this))
            ObserverService.RegisterObserver(this);

        _boardConfiguration = AreaService.GetAreaConfiguration().Boards
            .FirstOrDefault(b => b.BoardType == BoardType.RegionalForecast);
    }

    public void Update(object data)
    {
        foreach (var rfObject in _rfBoard.RfObjects.Values)
            rfObject.Update();
    }

    public void Dispose() => ObserverService.RemoveObserver(this);
}
