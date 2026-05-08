using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;

namespace GeoDa.Domain.CurrentForecasts.Services.SystemQualities;

internal class SystemQualityFactory : ISystemQualityFactory
{
    private readonly ILogger<SystemQualityFactory> _logger;

    public SystemQualityFactory(ILogger<SystemQualityFactory> logger)
    {
        _logger = logger;
    }

    // ISystemQualityFactory
    public SystemQualityInfo Create(
        int objectId,
        DateTime dateTimeReportPrepare,
        GpnsCountInfo gpnsCountInfo,
        SystemWorkTimeInfo workTimeInfo) =>
        new(ObjectId: objectId, DateTimeReportPrepare: dateTimeReportPrepare, GpnsCountInfo: gpnsCountInfo,
            SystemWorkTimeInfo: workTimeInfo);


    public SystemQualityInfo CreateDefault() =>
         new(ObjectId: -1,
             DateTimeReportPrepare: DateTime.MinValue,
             GpnsCountInfo: new(Quality.Uncertain, 0, DateTime.MinValue),
             SystemWorkTimeInfo: new(Quality.Uncertain, 0.0));
}
