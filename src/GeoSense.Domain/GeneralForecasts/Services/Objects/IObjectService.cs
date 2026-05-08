using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using System.Collections.Generic;

namespace GeoDa.Domain.GeneralForecasts.Services.Objects;

public interface IObjectService
{
    bool IsObjectExists(string objectName, IReadOnlyList<ObjectInfo> objectsInDb);

    public ObjectStatus GetObjectStatus(string objectName, IReadOnlyList<ObjectInfo> objectsInDb);

    ObjectInfo GetObjectInfoByName(string objectName, IReadOnlyList<ObjectInfo> objectsInDb);

    int GetObjectIdByName(string objectName, IReadOnlyList<ObjectInfo> objectsInDb);

    ObjectType GetObjectTypeByName(string objectName, IReadOnlyList<ObjectInfo> objectsInDb);

    string GetObjectNameById(int objectId, IReadOnlyList<ObjectInfo> objectsInDb);

    Dictionary<string, ObjectInfoAndStatus> GetObjectInfoAndStatuses(
        IReadOnlyList<string> objectNamesInSettings,
        IReadOnlyList<ObjectInfo> objectsInDb,
        ObjectStatus reasonNoDataInDb = ObjectStatus.Absent);

    Dictionary<string, int> GetExistedObjects(
        IReadOnlyList<string> objectNamesInSettings,
        IReadOnlyList<ObjectInfo> objectsInDb);

    Dictionary<string, int> GetNotExistedObjects(
        IReadOnlyList<string> objectNamesInSettings,
        IReadOnlyList<ObjectInfo> objectsInDb);

    bool IsAnyObjectAreNotFinded(
        IReadOnlyList<string> objectNamesInSettings,
        IReadOnlyList<ObjectInfo> objectsInDb);

    bool IsAllObjectsAreFinded(
        IReadOnlyList<string> objectNamesInSettings,
        IReadOnlyList<ObjectInfo> objectsInDb);
}
