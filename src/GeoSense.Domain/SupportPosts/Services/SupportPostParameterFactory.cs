using GeoDa.Domain.Models;
using GeoDa.Domain.SupportPosts.Models;

namespace GeoDa.Domain.SupportPosts.Services;

internal class SupportPostParameterFactory : ISupportPostParameterFactory
{
    public SupportPostParameter CreateDefault() =>
         new(Id: -1, Name: string.Empty, Unit: string.Empty, Value: 0.0f, Quality: Quality.Uncertain);

    public SupportPostParameter Create(int id, string name, string unit, float value, Quality quality) =>
         new(Id: id, Name: name, Unit: unit, Value: value, Quality: quality);
}
