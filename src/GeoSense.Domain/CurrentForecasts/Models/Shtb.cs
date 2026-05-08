using System.Linq;
using System.Text;

namespace GeoDa.Domain.CurrentForecasts.Models;

public class Shtb
{
    public const int MAX_COUNT_OF_SHTB_VALUES = 18;
    public int Obj { get; set; }
    public int Idat { get; set; }
    public int Hh { get; set; }
    public float Mo { get; set; }
    public int NrecAll { get; set; }
    public int NrecComb { get; set; }
    public int Dostover { get; set; }
    public int Ngp { get; set; }
    public int Nsum { get; set; }
    public int Nx { get; set; }
    public int Ny { get; set; }
    public float X1 { get; set; }
    public float Y1 { get; set; }
    public float Dx { get; set; }
    public float Dy { get; set; }
    public float[] Alarms { get; set; } = new float[MAX_COUNT_OF_SHTB_VALUES];
    public float[] Values { get; set; } = new float[MAX_COUNT_OF_SHTB_VALUES];

    public override string ToString() =>
        (new StringBuilder()).Append($"{GetType().Name}() {{")
                    .AppendJoin(", ", GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}"))
                    .Append("}")
                    .ToString();
}
