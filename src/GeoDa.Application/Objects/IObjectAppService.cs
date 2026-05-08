using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using System.Collections.Generic;

namespace GeoDa.Application.Objects;

internal interface IObjectAppService
{
    void SetDbConnectionString(string dbConnString);

    (ObjectStatus Status, int Id) GetObjectStatusAndId(string objectName);

    Dictionary<string, ObjectInfoAndStatus> GetObjectInfoAndStatuses(IReadOnlyList<string> objectsFromSettings);
}
