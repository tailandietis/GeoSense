using GeoDa.Domain.DigitalRepers.Models;
using GeoDa.Domain.Models;
using System;
using System.Collections.Generic;

namespace GeoDa.Domain.DigitalRepers.Services;

internal class DigitalReperDataFactory : IDigitalReperDataFactory
{
    public DigitalReperData CreateDefault() =>
        new(ObjectId: -1,
            ObjectName: string.Empty,
            ObjectDataQuality: Quality.Uncertain,
            DateTime: DateTime.MinValue,
            Parameters: new());

    public DigitalReperData Create(
        int objectId,
        string objectName,
        Quality objectQuality,
        DateTime dateTime,
        List<DigitalReperParameter> parameters) =>
        new(ObjectId: objectId,
            ObjectName: objectName,
            ObjectDataQuality: objectQuality,
            DateTime: dateTime,
            Parameters: parameters);
}
