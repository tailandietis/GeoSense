using System;

namespace GeoDa.Infrastructure.Exceptions;

public class GeoDaInfrastructureException : Exception
{
    public GeoDaInfrastructureException(string message)
        : base(message) { }
}
