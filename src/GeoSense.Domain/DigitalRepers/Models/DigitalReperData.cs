using GeoDa.Domain.Models;
using System;
using System.Collections.Generic;

namespace GeoDa.Domain.DigitalRepers.Models;

public record DigitalReperData(
    int ObjectId,
    string ObjectName,
    Quality ObjectDataQuality,
    DateTime DateTime,
    List<DigitalReperParameter> Parameters);