using GeoDa.Domain.Combines.Models;
using GeoDa.Domain.Models;
using System;
using System.Collections.Generic;

namespace GeoDa.Domain.Combines.Services;

public interface ICombineDataFactory
{
    CombineData CreateDefault();

    CombineData Create(
        int objectId,
        string objectName,
        Quality objectQuality,
        DateTime dateTime,
        List<CombineParameter> parameters);
}
