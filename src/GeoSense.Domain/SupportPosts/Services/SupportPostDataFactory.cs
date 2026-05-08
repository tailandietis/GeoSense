using GeoDa.Domain.Models;
using GeoDa.Domain.SupportPosts.Models;
using System;
using System.Collections.Generic;

namespace GeoDa.Domain.SupportPosts.Services;

internal class SupportPostDataFactory : ISupportPostDataFactory
{
    public SupportPostData CreateDefault() =>
        new(ObjectId: -1,
            ObjectName: string.Empty,
            ObjectDataQuality: Quality.Uncertain,
            DateTime: DateTime.MinValue,
            Parameters: new());

    public SupportPostData Create(
        int objectId,
        string objectName,
        Quality objectDataQuality,
        DateTime dateTime,
        List<SupportPostParameter> parameters) =>
        new(ObjectId: objectId,
            ObjectName: objectName,
            ObjectDataQuality: objectDataQuality,
            DateTime: dateTime,
            Parameters: parameters);
}
