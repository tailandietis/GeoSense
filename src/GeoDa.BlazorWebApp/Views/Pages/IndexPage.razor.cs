using GeoDa.BlazorWebApp.Services.MLBuilder;
using Microsoft.AspNetCore.Components;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class IndexPage : ComponentBase
{
    [Inject] private IMLBuilderService MLBuilderService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private MLTrainResult? _mlResult;
    private bool _rfConnected = false;

    protected override void OnInitialized()
    {
        if (MLBuilderService.IsTrainResultReady())
            _mlResult = MLBuilderService.GetLastTrainResult();
    }

    private void Navigate(string url) => NavigationManager.NavigateTo(url);
}
