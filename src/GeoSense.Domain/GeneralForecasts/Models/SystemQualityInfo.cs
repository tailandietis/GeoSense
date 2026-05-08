using GeoDa.Domain.Models;
using System;

namespace GeoDa.Domain.GeneralForecasts.Models;

public record SystemQualityInfo(
    int ObjectId,
    DateTime DateTimeReportPrepare,
    GpnsCountInfo GpnsCountInfo,
    SystemWorkTimeInfo SystemWorkTimeInfo);

public record GpnsCountInfo(Quality Quality, int Value, DateTime DateTime);

public record SystemWorkTimeInfo(Quality Quality, double Value);