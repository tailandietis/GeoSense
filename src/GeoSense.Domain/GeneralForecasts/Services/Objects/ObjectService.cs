using GeoDa.Domain.Exceptions;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;


namespace GeoDa.Domain.GeneralForecasts.Services.Objects;

internal class ObjectService : IObjectService
{
    private readonly ILogger<ObjectService> _logger;

    public ObjectService(ILogger<ObjectService> logger)
    {
        _logger = logger;
    }

    // IObjectsService

    public bool IsObjectExists(string objectName, IReadOnlyList<ObjectInfo> objectsInDb)
    {
        var result = objectsInDb.Any(v => v.Name == objectName);

        return result;
    }

    public ObjectStatus GetObjectStatus(string objectName, IReadOnlyList<ObjectInfo> objectsInDb)
    {
        if (objectsInDb.Count > 0)
        {
            var dpls = objectsInDb.Select(v => v.Name.Trim())
                           .GroupBy(v => v)
                           .Where(v => v.Count() > 1)
                           .Select(v => v.Key)
                           .ToList();

            var data = objectsInDb.Where(v => !dpls.Contains(v.Name))
                            .ToDictionary(v => v.Name, v => (v.Id, v.Type));

            if (data.ContainsKey(objectName))
                return ObjectStatus.Ok;
            else if (dpls.Contains(objectName))
                return ObjectStatus.HasDuplicate;
            else
                return ObjectStatus.Absent;
        }
        else
        {
            return ObjectStatus.Absent;
        }
    }

    public ObjectInfo GetObjectInfoByName(string objectName, IReadOnlyList<ObjectInfo> objectsInDb)
    {
        var objectInfo = objectsInDb.Where(x => x.Name == objectName).FirstOrDefault();

        if (objectInfo == null)
            throw new GeoDaDomainException(OpStatus.UnknownObjectName, $"Object {objectName} is absent");

        return objectInfo;
    }

    public int GetObjectIdByName(string objectName, IReadOnlyList<ObjectInfo> objectsInDb)
    {
        var objs = objectsInDb
            .Where(x => x.Name == objectName)
            .ToList();

        return objs.Count switch
        {
            0 => throw new GeoDaDomainException(OpStatus.UnknownObjectName, $"Object {objectName} is absent"),
            1 => objs.First().Id,
            _ => throw new GeoDaDomainException(OpStatus.DuplicateDataError, $"Object {objectName} is duplicate"),
        };
    }

    public ObjectType GetObjectTypeByName(string objectName, IReadOnlyList<ObjectInfo> objectsInDb)
    {
        var objs = objectsInDb
            .Where(x => x.Name == objectName)
            .ToList();

        return objs.Count switch
        {
            0 => throw new GeoDaDomainException(OpStatus.UnknownObjectName, $"Object {objectName} is absent"),
            1 => objs.First().Type,
            _ => throw new GeoDaDomainException(OpStatus.DuplicateDataError, $"Object {objectName} is duplicate"),
        };
    }

    public string GetObjectNameById(int objectId, IReadOnlyList<ObjectInfo> objectsInDb)
    {
        var objs = objectsInDb
            .Where(x => x.Id == objectId)
            .ToList();

        return objs.Count switch
        {
            0 => throw new GeoDaDomainException(OpStatus.UnknownObjectName, $"Object {objectId} is absent"),
            1 => objs.First().Name,
            _ => throw new GeoDaDomainException(OpStatus.DuplicateDataError, $"Object {objectId} is duplicate"),
        };
    }

    public Dictionary<string, ObjectInfoAndStatus> GetObjectInfoAndStatuses(
        IReadOnlyList<string> objectNamesInSettings,
        IReadOnlyList<ObjectInfo> objectsInDb,
        ObjectStatus reasonNoDataInDb = ObjectStatus.Absent)
    {
        var result = new Dictionary<string, ObjectInfoAndStatus>();

        if (objectsInDb.Count > 0)
        {
            var dpls = objectsInDb.Select(v => v.Name.Trim())
                           .GroupBy(v => v)
                           .Where(v => v.Count() > 1)
                           .Select(v => v.Key)
                           .ToList();

            var data = objectsInDb.Where(v => !dpls.Contains(v.Name))
                            .ToDictionary(v => v.Name, v => (v.Id, v.Type));

            foreach (var objName in objectNamesInSettings)
            {
                if (data.ContainsKey(objName))
                {
                    var objectInfo = new ObjectInfo(Id: data[objName].Id, Type: data[objName].Type, Name: objName);
                    result.Add(objName, new() { ObjectInfo = objectInfo, Status = ObjectStatus.Ok });
                }
                else if (dpls.Contains(objName))
                {
                    var objectInfo = new ObjectInfo(Id: -1, Type: ObjectType.Uncertain, Name: string.Empty);
                    result.Add(objName, new() { ObjectInfo = objectInfo, Status = ObjectStatus.HasDuplicate });
                }
                else
                {
                    var objectInfo = new ObjectInfo(Id: -1, Type: ObjectType.Uncertain, Name: string.Empty);
                    result.Add(objName, new() { ObjectInfo = objectInfo, Status = ObjectStatus.Absent });
                }
            }
        }
        else
        {
            foreach (var obj in objectNamesInSettings)
            {
                var objectInfo = new ObjectInfo(Id: -1, Type: ObjectType.Uncertain, Name: string.Empty);
                result.Add(obj, new() { ObjectInfo = objectInfo, Status = reasonNoDataInDb });
            }
        }

        return result;
    }

    public Dictionary<string, int> GetExistedObjects(
        IReadOnlyList<string> objectNamesInSettings,
        IReadOnlyList<ObjectInfo> objectsInDb)
    {
        var result = GetObjectInfoAndStatuses(objectNamesInSettings, objectsInDb)
            .Where(v => v.Value.Status == ObjectStatus.Ok)
            .ToDictionary(v => v.Key, v => v.Value.ObjectInfo.Id);

        return result;
    }

    public Dictionary<string, int> GetNotExistedObjects(
        IReadOnlyList<string> objectNamesInSettings,
        IReadOnlyList<ObjectInfo> objectsInDb)
    {
        var result = GetObjectInfoAndStatuses(objectNamesInSettings, objectsInDb)
            .Where(v => v.Value.Status != ObjectStatus.Ok)
            .ToDictionary(v => v.Key, v => v.Value.ObjectInfo.Id);

        return result;
    }

    public bool IsAnyObjectAreNotFinded(
        IReadOnlyList<string> objectNamesInSettings,
        IReadOnlyList<ObjectInfo> objectsInDb)
    {
        var objs = GetObjectInfoAndStatuses(objectNamesInSettings, objectsInDb);

        var result = objs.Any(item => item.Value.Status != ObjectStatus.Ok);

        return result;
    }

    public bool IsAllObjectsAreFinded(
        IReadOnlyList<string> objectNamesInSettings,
        IReadOnlyList<ObjectInfo> objectsInDb)
    {
        var isAnyFinded = IsAnyObjectAreNotFinded(objectNamesInSettings, objectsInDb);

        return !isAnyFinded;
    }
}
