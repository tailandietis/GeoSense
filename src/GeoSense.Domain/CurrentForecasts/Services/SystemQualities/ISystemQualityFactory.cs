using GeoDa.Domain.GeneralForecasts.Models;
using System;

namespace GeoDa.Domain.CurrentForecasts.Services.SystemQualities;

public interface ISystemQualityFactory
{
    SystemQualityInfo Create(
        int objectId,
        DateTime dateTimeReportPrepare,
        GpnsCountInfo gpnsCountInfo,
        SystemWorkTimeInfo workTimeInfo);

    SystemQualityInfo CreateDefault();
}
