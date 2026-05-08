using System.Linq;
using System.Text;

namespace GeoDa.Application.GeneralForecasts.Repository.Geophones.Dtos;

public class GeophoneDto
{
    public int Obj { get; set; }
    public int Num { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public override string ToString() =>
        new StringBuilder().Append($"{GetType().Name}() {{")
                    .AppendJoin(", ", GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}"))
                    .Append("}")
                    .ToString();
}
