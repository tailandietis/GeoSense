using GeoDa.Domain.DigitalRepers.Models;
using GeoDa.Domain.Models;

namespace GeoDa.Domain.DigitalRepers.Services;

internal class DigitalReperParameterFactory : IDigitalReperParameterFactory
{
    public DigitalReperParameter CreateDefault() =>
         new(Id: -1, Name: string.Empty, Unit: string.Empty, Value: 0.0f, Quality: Quality.Uncertain);

    public DigitalReperParameter Create(int id, string name, string unit, float value, Quality quality) =>
         new(Id: id, Name: name, Unit: unit, Value: value, Quality: quality);
}
