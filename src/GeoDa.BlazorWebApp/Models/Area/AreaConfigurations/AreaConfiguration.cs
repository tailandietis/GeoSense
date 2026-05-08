using GeoDa.BlazorWebApp.Models.Area.BoardConfigurations;
using System.Collections.Generic;

namespace GeoDa.BlazorWebApp.Models.Area.AreaConfigurations;

public class AreaConfiguration
{
    public List<BoardConfiguration> Boards { get; set; } = new();
}
