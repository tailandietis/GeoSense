using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using System;
using System.Collections.Generic;

namespace GeoDa.Application.RegionalForecasts.Services.DensityMaps;

public interface IRfDensityMapsService
{
    bool CreateDensityMapsImages(
        int objectId,
        DateTime datetime,
        RegionalForecastObjectSettings settings,
        IReadOnlyList<Event> events);

    Dictionary<DensityMapType, string> GetDensityMapsImagesName(int objectId, DateTime dateTime);
}
