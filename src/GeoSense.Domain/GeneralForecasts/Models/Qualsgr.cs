using System.Linq;
using System.Text;

namespace GeoDa.Domain.GeneralForecasts.Models;

public class Qualsgr
{
    public int Obj { get; set; }
    public int Idat { get; set; }
    public int Itim { get; set; }
    public int Qc { get; set; }
    public int Err { get; set; }
    public int Ngp { get; set; }
    public int Nsam { get; set; }
    public float Si { get; set; }
    public int Condit { get; set; }
    public int Gain { get; set; }
    public int Filt { get; set; }
    public int InclUsed { get; set; }
    public float Amid { get; set; }
    public float Amax { get; set; }

    public override string ToString() =>
        new StringBuilder().Append($"{GetType().Name}() {{")
                    .AppendJoin(", ", GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}"))
                    .Append("}")
                    .ToString();
}
