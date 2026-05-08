using GeoDa.Domain.Combines.Models;
using GeoDa.Domain.Models;

namespace GeoDa.Domain.Combines.Services;

internal class CombineParameterFactory : ICombineParameterFactory
{
    public CombineParameter CreateDefault() =>
         new(Id: -1, Name: string.Empty, Unit: string.Empty, Value: 0.0f, Quality: Quality.Uncertain);

    public CombineParameter Create(int id, string name, string unit, float value, Quality quality) =>
         new(Id: id, Name: name, Unit: unit, Value: value, Quality: quality);
}
