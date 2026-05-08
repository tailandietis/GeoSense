using System;
using System.Linq;
using System.Text;

namespace GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies.Dtos;

public class GsParamFEnergyDto
{
    public int Obj { get; set; }
    public DateTime Dt { get; set; }
    public int ParamFStatus { get; set; }
    public int EnergyStatus { get; set; }

    public override string ToString() =>
        new StringBuilder().Append($"{GetType().Name}() {{")
                    .AppendJoin(", ", GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}"))
                    .Append("}")
                    .ToString();
}
