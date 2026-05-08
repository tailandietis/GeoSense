using GeoDa.Domain.Models;

namespace GeoDa.BlazorWebApp.Models.SystemObjects;

public class ObjectData
{
    public string Name { get; set; } = string.Empty;

    public string StatusDiscription { get; set; } = string.Empty;

    public ObjectStatus Status { get; set; } = ObjectStatus.Uncertain;
}
