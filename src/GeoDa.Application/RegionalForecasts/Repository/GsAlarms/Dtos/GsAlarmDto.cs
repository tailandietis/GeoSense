using System;
using System.Linq;
using System.Text;

namespace GeoDa.Application.RegionalForecasts.Repository.GsAlarms.Dtos;

public class GsAlarmDto
{
    public int Obj { get; set; }
    public DateTime Dt { get; set; }
    public int Quality { get; set; }
    public bool AlarmStatus { get; set; }
    public DateTime ItemDt { get; set; }
    public float E { get; set; }
    public float ELim { get; set; }
    public int? CheckInterval { get; set; }

    public override string ToString() =>
        new StringBuilder().Append($"{GetType().Name}() {{")
                    .AppendJoin(", ", GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}"))
                    .Append("}")
                    .ToString();
}
