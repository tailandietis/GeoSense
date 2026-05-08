using GeoDa.Domain.GeneralForecasts.Models;

namespace GeoDa.Domain.GeneralForecasts.Services.Objects;

public class ObjectInfoFactory : IObjectInfoFactory
{
    public ObjectInfo CreateDefault() =>
        new(Id: -1, Type: ObjectType.Uncertain, Name: string.Empty);

    public ObjectInfo Create(int id, ObjectType type, string name) =>
        new(Id: id, Type: type, Name: name);
}
