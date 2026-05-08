using GeoDa.Domain.Models;

namespace GeoDa.Domain.GeneralForecasts.Models;

public struct ObjectInfoAndStatus
{
    public ObjectInfoAndStatus(ObjectInfo objectInfo, ObjectStatus objectStatus)
    {
        ObjectInfo = objectInfo;
        Status = objectStatus;
    }

    public ObjectInfo ObjectInfo { get; set; }

    public ObjectStatus Status { get; set; }
}
