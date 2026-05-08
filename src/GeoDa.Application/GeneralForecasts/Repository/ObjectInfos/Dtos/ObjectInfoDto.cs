using System.Linq;
using System.Text;

namespace GeoDa.Application.GeneralForecasts.Repository.ObjectInfos.Dtos;

public class ObjectInfoDto
{
    public int Obj { get; set; }
    public int ObjTyp { get; set; }
    public string ObjName { get; set; } = "";

    public override string ToString() =>
        new StringBuilder().Append($"{GetType().Name}() {{")
                    .AppendJoin(", ", GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}"))
                    .Append("}")
                    .ToString();
}
