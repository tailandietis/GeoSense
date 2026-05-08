using Microsoft.AspNetCore.Components;

namespace GeoDa.BlazorWebApp.Views.Bases;

public partial class HeaderBase
{
    [Parameter]
    public string Text { get; set; } = string.Empty;

    [Parameter]
    public int Level { get; set; } = 1;

    [Parameter]
    public bool IsNeedDivider { get; set; } = true;
}
