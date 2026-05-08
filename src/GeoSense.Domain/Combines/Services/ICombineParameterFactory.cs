using GeoDa.Domain.Combines.Models;
using GeoDa.Domain.Models;

namespace GeoDa.Domain.Combines.Services;

public interface ICombineParameterFactory
{
    CombineParameter CreateDefault();

    CombineParameter Create(int id, string name, string unit, float value, Quality quality);
}
