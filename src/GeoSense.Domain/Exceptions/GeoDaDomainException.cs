using GeoDa.Domain.Models;
using System;

namespace GeoDa.Domain.Exceptions;

public class GeoDaDomainException : Exception
{
    public OpStatus OperationStatus { get; }

    public GeoDaDomainException(OpStatus status, string message)
        : base(message)
    {
        OperationStatus = status;
    }
}
