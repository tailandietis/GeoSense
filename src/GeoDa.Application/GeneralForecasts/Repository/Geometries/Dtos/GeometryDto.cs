using System.Linq;
using System.Text;

namespace GeoDa.Application.GeneralForecasts.Repository.Geometries.Dtos;

public class GeometryDto
{
    public int Obj { get; set; }
    public int NgpMax { get; set; }
    public float XMin { get; set; }
    public float XMax { get; set; }
    public float YMin { get; set; }
    public float YMax { get; set; }
    public float WShtrek { get; set; }
    public int ZbDir { get; set; }
    public float ZMin { get; set; }
    public float ZMax { get; set; } 

    public override string ToString() =>
        new StringBuilder().Append($"{GetType().Name}() {{")
                    .AppendJoin(", ", GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}"))
                    .Append("}")
                    .ToString();
}
