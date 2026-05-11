using GeoDa.BlazorWebApp.Services.MLBuilder;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class MLEventsPage : ComponentBase
{
    [Inject] private IMLBuilderService MLBuilderService { get; set; } = default!;
    [Inject] private MLObjectStateService ObjectState { get; set; } = default!;
    [Inject] private XSeisContextService XSeisService { get; set; } = default!;

    private bool _modelReady;
    private bool _isRunning;
    private string? _errorMessage;
    private MLPredictResult? _result;
    private List<PredictedEvent> _filtered = new();
    private int _filterClass = -1;
    private XSeisMassifStateDto? _xseisState;
    private DateTime?[] _dateRange = new DateTime?[]
    {
        DateTime.Now.AddDays(-30),
        DateTime.Now
    };

    protected override void OnInitialized()
    {
        _modelReady = MLBuilderService.IsTrainResultReady();
        ObjectState.GetObjects();
    }

    private async Task RunPredict()
    {
        if (ObjectState.SelectedObjId is null)
        {
            _errorMessage = "Выберите объект в ML Dashboard";
            return;
        }

        _isRunning = true;
        _errorMessage = null;
        StateHasChanged();

        var start = _dateRange[0] ?? DateTime.Now.AddDays(-30);
        var end   = _dateRange[1] ?? DateTime.Now;

        _result = await MLBuilderService.PredictAsync(ObjectState.SelectedObjId.Value, start, end);
        _xseisState = await XSeisService.GetLatestMassifStateAsync(ObjectState.SelectedObjId.Value);

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

    private void OnDateRangeChanged(DateTime?[] value)
    {
        _dateRange = value;
    }
}
