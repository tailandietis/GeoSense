using Microsoft.AspNetCore.Components;

namespace GeoDa.BlazorWebApp.Views.Bases;

public partial class ButtonBase
{
    [Parameter]
    public string Text { get; set; } = string.Empty;
}
