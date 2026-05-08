using GeoDa.BlazorWebApp.Models.Helps;
using HelpDataModel = GeoDa.BlazorWebApp.Models.Helps.HelpData;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace GeoDa.BlazorWebApp.Views.Pages.Helps;

public partial class HelpSelect
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private List<HelpData> HelpData { get; set; }

    public HelpSelect()
    {
        HelpData = new()
        {
            new() { Id = HelpDataModel.MsMethodologyId, Name = "Методика риска возникновения ДЯ" },
            new() { Id = HelpDataModel.MikonGeoMethodologyId, Name = "Методика оценки удароопасности «Микон-Гео»" }
        };
    }

    // Private
    private string GetTitle() =>
        "Справка";

    private void GoToExtViewPage(int id) =>
        NavigationManager.NavigateTo($"methodology/{id}");
}
