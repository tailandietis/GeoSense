using System.Linq;
using System.Text;

namespace GeoDa.Application.GeneralForecasts.Repository.Qualgps.Dtos;

public class QualgpDto
{
    public int Obj { get; set; }
    public int Idat { get; set; }
    public int Itim { get; set; }
    public int Gpnum { get; set; }
    public int? Qc { get; set; }
    public int? XQc { get; set; }
    public int? XErr { get; set; }
    public float? XA { get; set; }
    public float? XAmax { get; set; }
    public float? XF { get; set; }
    public float? XFw { get; set; }
    public float? XS1 { get; set; }
    public float? XS2 { get; set; }
    public float? XS3 { get; set; }
    public float? XS4 { get; set; }
    public float? XS5 { get; set; }
    public float? XInc { get; set; }
    public int? YQc { get; set; }
    public int? YErr { get; set; }
    public float? YA { get; set; }
    public float? YAmax { get; set; }
    public float? YF { get; set; }
    public float? YFw { get; set; }
    public float? YS1 { get; set; }
    public float? YS2 { get; set; }
    public float? YS3 { get; set; }
    public float? YS4 { get; set; }
    public float? YS5 { get; set; }
    public float? YInc { get; set; }
    public int? ZQc { get; set; }
    public int? ZErr { get; set; }
    public float? ZA { get; set; }
    public float? ZAmax { get; set; }
    public float? ZF { get; set; }
    public float? ZFw { get; set; }
    public float? ZS1 { get; set; }
    public float? ZS2 { get; set; }
    public float? ZS3 { get; set; }
    public float? ZS4 { get; set; }
    public float? ZS5 { get; set; }

    public override string ToString() =>
        new StringBuilder().Append($"{GetType().Name}() {{")
                    .AppendJoin(", ", GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}"))
                    .Append("}")
                    .ToString();
}
