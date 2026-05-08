using GeoDa.Domain.GeneralForecasts.Models;

namespace GeoDa.Domain.GeneralForecasts.Services.Objects;

public interface IObjectInfoFactory
{
    ObjectInfo CreateDefault();
    ObjectInfo Create(int id, ObjectType type, string name);
}
