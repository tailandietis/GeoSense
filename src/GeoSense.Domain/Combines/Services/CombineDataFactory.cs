using GeoDa.Domain.Combines.Models;
using GeoDa.Domain.Models;
using System;
using System.Collections.Generic;

namespace GeoDa.Domain.Combines.Services;

internal class CombineDataFactory : ICombineDataFactory
{
    public CombineData CreateDefault() =>
        new(ObjectId: -1,
            ObjectName: string.Empty,
            ObjectDataQuality: Quality.Uncertain,
            DateTime: DateTime.MinValue,
            Parameters: new());

    public CombineData Create(
        int objectId,
        string objectName,
        Quality objectQuality,
        DateTime dateTime,
        List<CombineParameter> parameters) =>
        new(ObjectId: objectId,
            ObjectName: objectName,
            ObjectDataQuality: objectQuality,
            DateTime: dateTime,
            Parameters: parameters);
}
