using GeoDa.BlazorWebApp.Models.Gui.Colors;
using Microsoft.AspNetCore.Components;


namespace GeoDa.BlazorWebApp.Views.Bases;

public partial class TextBase
{
    [Parameter]
    public string Text { get; set; } = string.Empty;

    [Parameter]
    public GeoDaColor Color { get; set; } = GeoDaColor.Black;

    [Parameter]
    public string FontSize { get; set; } = "12px";

    [Parameter]
    public string FontWeight { get; set; } = "normal";

    [Parameter]
    public GeoDaColor BgColor { get; set; } = GeoDaColor.Transparent;

    [Parameter]
    public string Id { get; set; } = string.Empty;
}
