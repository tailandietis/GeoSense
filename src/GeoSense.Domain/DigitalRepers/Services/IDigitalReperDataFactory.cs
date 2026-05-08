using GeoDa.Domain.DigitalRepers.Models;
using GeoDa.Domain.Models;
using System;
using System.Collections.Generic;

namespace GeoDa.Domain.DigitalRepers.Services;

public interface IDigitalReperDataFactory
{
    DigitalReperData CreateDefault();

    DigitalReperData Create(
        int objectId,
        string objectName,
        Quality objectQuality,
        DateTime dateTime,
        List<DigitalReperParameter> parameters);
}
