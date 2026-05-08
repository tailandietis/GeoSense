using GeoDa.BlazorWebApp.Services.MLBuilder;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class MLEventsPage : ComponentBase
{
    [Inject] private IMLBuilderService MLBuilderService { get; set; } = default!;

    private bool _modelReady;
    private bool _isRunning;
    private string? _errorMessage;
    private MLPredictResult? _result;
    private List<PredictedEvent> _filtered = new();
    private int _filterClass = -1;

    protected override void OnInitialized()
    {
        _modelReady = MLBuilderService.IsTrainResultReady();

        if (MLBuilderService.IsPredictResultReady())
        {
            _result = MLBuilderService.GetLastPredictResult();
            ApplyFilter();
        }
    }

    private async Task RunPredict()
    {
        _isRunning = true;
        _errorMessage = null;
        StateHasChanged();

        _result = await MLBuilderService.PredictDemoAsync();

        if (!_result.Success)
            _errorMessage = _result.ErrorMessage;
        else
            ApplyFilter();

        _isRunning = false;
        StateHasChanged();
    }

    private void ApplyFilter()
    {
        if (_result == null) return;

        _filtered = _filterClass == -1
            ? _result.Events.ToList()
            : _result.Events.Where(e => e.Label == _filterClass).ToList();
    }
}
