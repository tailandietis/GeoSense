namespace GeoDa.Domain.Models;

public enum ObjectStatus
{
    Uncertain,

    Ok,

    DbError,
    Absent,
    HasDuplicate,
    NotConnected
}
