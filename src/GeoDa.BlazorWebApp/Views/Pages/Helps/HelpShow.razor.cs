using Microsoft.AspNetCore.Components;

namespace GeoDa.BlazorWebApp.Views.Pages.Helps;

public partial class HelpShow
{
    [Parameter]
    public string MethodologyId { get; set; } = string.Empty;
}
