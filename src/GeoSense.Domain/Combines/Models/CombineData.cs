using GeoDa.Domain.Models;
using System;
using System.Collections.Generic;

namespace GeoDa.Domain.Combines.Models;

public record CombineData(
    int ObjectId,
    string ObjectName,
    Quality ObjectDataQuality,
    DateTime DateTime,
    List<CombineParameter> Parameters);