using GeoDa.Domain.Models;
using System;

namespace GeoDa.Application.Exceptions;

public class GeoDaAppException : Exception
{
    public OpStatus OperationStatus { get; }

    public GeoDaAppException(OpStatus status, string message)
        : base(message)
    {
        OperationStatus = status;
    }
}
