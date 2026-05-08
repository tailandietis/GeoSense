using System;
using System.Linq;
using System.Text;

namespace GeoDa.Application.RegionalForecasts.Repository.GsStats.Dtos;

public class GsStatDto
{
    public int Obj { get; set; }
    public DateTime Dt { get; set; }
    public int Quality { get; set; }
    public float MinVal { get; set; }
    public float MaxVal { get; set; }
    public float Q70 { get; set; }
    public float Q80 { get; set; }
    public float Q90 { get; set; }
    public float Q95 { get; set; }
    public float Q99 { get; set; }
    public int NRows { get; set; }
    public DateTime DtOldest { get; set; }
    public DateTime DtNewest { get; set; }
    public int StatCalcInterval { get; set; }

    public override string ToString() =>
        new StringBuilder().Append($"{GetType().Name}() {{")
                    .AppendJoin(", ", GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}"))
                    .Append("}")
                    .ToString();
}
