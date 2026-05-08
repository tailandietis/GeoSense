using GeoDa.BlazorWebApp.Services.MLBuilder;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class MlVsGeoDaPage : ComponentBase
{
    [Inject] private IMLBuilderService MLBuilderService { get; set; } = default!;

    private bool _hasData = false;
    private bool _isLoading = false;
    private DateTime?[] _dateRange = new DateTime?[] { DateTime.Now.AddDays(-7), DateTime.Now };
    private string _filterMode = "all";

    private int _totalEvents = 0;
    private int _matchCount = 0;
    private int _mlHigherCount = 0;
    private int _geodaHigherCount = 0;
    private double _matchRate => _totalEvents > 0 ? (double)_matchCount / _totalEvents : 0;

    private List<ComparisonRow> _allRows = new();
    private List<ComparisonRow> _filteredRows = new();

    protected override Task OnInitializedAsync() => LoadData();

    private async Task LoadData()
    {
        _isLoading = true;
        StateHasChanged();

        await Task.Delay(100);

        // TODO: заменить на реальные данные из БД + predict когда придут слепки
        _allRows = new List<ComparisonRow>();
        _hasData = false;

        ApplyFilter();
        UpdateStats();

        _isLoading = false;
        StateHasChanged();
    }

    private void ApplyFilter()
    {
        _filteredRows = _filterMode switch
        {
            "mismatch" => _allRows.Where(r => !r.IsMatch).ToList(),
            "ml_higher" => _allRows.Where(r => r.MlClassIndex > r.GeoDaClassIndex).ToList(),
            "geoda_higher" => _allRows.Where(r => r.GeoDaClassIndex > r.MlClassIndex).ToList(),
            _ => _allRows.ToList()
        };
    }

    private void UpdateStats()
    {
        _totalEvents = _allRows.Count;
        _matchCount = _allRows.Count(r => r.IsMatch);
        _mlHigherCount = _allRows.Count(r => r.MlClassIndex > r.GeoDaClassIndex);
        _geodaHigherCount = _allRows.Count(r => r.GeoDaClassIndex > r.MlClassIndex);
    }
}

public record ComparisonRow(
    string DateTime,
    string Energy,
    string X,
    string Y,
    string Z,
    string GeoDaLevel,
    string GeoDaColor,
    int GeoDaClassIndex,
    string MlLevel,
    string MlColor,
    int MlClassIndex)
{
    public bool IsMatch => GeoDaClassIndex == MlClassIndex;
}
