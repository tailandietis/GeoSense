using System.Collections.Generic;

namespace GeoDa.BlazorWebApp.Models.Area.BoardConfigurations;

public class BoardConfiguration
{
    public BoardType BoardType { get; set; }

    public string Title { get; set; } = string.Empty;

    public List<string> ObjectNames { get; set; } = new();
}
