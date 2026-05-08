using GeoDa.Domain.DigitalRepers.Models;
using GeoDa.Domain.Models;

namespace GeoDa.Domain.DigitalRepers.Services;

public interface IDigitalReperParameterFactory
{
    DigitalReperParameter CreateDefault();

    DigitalReperParameter Create(int id, string name, string unit, float value, Quality quality);
}
