using System;
using System.Linq;
using System.Text;

namespace GeoDa.Application.GeneralForecasts.Repository.GsParams.Dtos;

public class GsParamDto
{
    public long Id { get; set; }
    public string ObjName { get; set; } = string.Empty;
    public DateTime Dt { get; set; }
    public string ParName { get; set; } = string.Empty;
    public string ParVal { get; set; } = string.Empty;

    public bool Equals(GsParamDto? other)
    {
        if (other is null)
            return false;

        return Id == other.Id
            && ObjName == other.ObjName
            && Dt == other.Dt
            && ParName == other.ParName
            && ParVal == other.ParVal;
    }

    public override bool Equals(object? obj) =>
        Equals(obj as GsParamDto);

    public override int GetHashCode() =>
        ToString().GetHashCode();

    public override string ToString() =>
        new StringBuilder().Append($"{GetType().Name}() {{")
                    .AppendJoin(", ", GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}"))
                    .Append("}")
                    .ToString();
}
