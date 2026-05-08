namespace GeoDa.Domain.Models;

public enum OpStatus
{
    Uncertain,

    Ok,

    ConfigurationError,

    NoData,
    BadData,

    NullPropertyInDataError,
    CastModelError,

    DuplicateDataError,

    GetDataError,
    InsertDataError,
    DeleteDataError,

    BadObject,
    UnknownObjectName,
    UnkonwnError
}
