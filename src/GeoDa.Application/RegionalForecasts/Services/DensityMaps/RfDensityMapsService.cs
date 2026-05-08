using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using GeoDa.Domain.RegionalForecasts.Services.BlocksInfoMaps;
using GeoDa.Infrastructure.Services.JsonFiles;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace GeoDa.Application.RegionalForecasts.Services.DensityMaps;

public partial class RfDensityMapsService : IRfDensityMapsService
{
    private readonly IRfDensityMapsDomainService _mapImageDomainService;

    private readonly IJsonFileService _jsonFileService;

    private readonly ILogger<RfDensityMapsService> _logger;

    public RfDensityMapsService(
        IRfDensityMapsDomainService mapImageService,
        IJsonFileService jsonFileService,
        ILogger<RfDensityMapsService> logger)
    {
        _mapImageDomainService = mapImageService;
        _jsonFileService = jsonFileService;

        _logger = logger;
    }

    // IRegionalForecastDensityMapsService
    public bool CreateDensityMapsImages(
        int objectId,
        DateTime dateTime,
        RegionalForecastObjectSettings settings,
        IReadOnlyList<Event> events)
    {
        var dirData = Path.Combine("tmp", "maps");
        var webRootImg = Path.Combine("wwwroot", "img");

        // Files with data
        var eventsFileName = _mapImageDomainService.GetEventsDataFileName(objectId, dirData, dateTime);
        var mapSettingsFileName = _mapImageDomainService.GetMapBuilderSettingsFileName(objectId, dirData, dateTime);

        // Images file names
        var densEventsMapImageFileName = _mapImageDomainService.GetDensityEventsMapImageFileName(objectId,
            webRootImg, dateTime);
        var densEventsMapWithLmitImageFileName = _mapImageDomainService.GetDensityEventsMapWithLimitImageFileName(
            objectId, webRootImg, dateTime);
        var densEnergyImageFileName = _mapImageDomainService.GetDensityEnergyMapImageFileName(objectId,
            webRootImg, dateTime);

        var images = new List<string>
            {
                densEventsMapImageFileName,
                densEventsMapWithLmitImageFileName,
                densEnergyImageFileName
            };

        try
        {
            ClearDataDirectory(objectId, dirData);
            CreateEventsFile(eventsFileName, events);
            CreateMapSettingsFile(mapSettingsFileName, images, settings);

            BuildMapsImages(eventsFileName, mapSettingsFileName);
            RemoveOldImg(objectId, webRootImg, images);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return false;
        }

        return true;
    }

    public Dictionary<DensityMapType, string> GetDensityMapsImagesName(int objectId, DateTime dateTime)
    {
        var densEventsMapImageFileName = _mapImageDomainService.GetDensityEventsMapImageFileName(objectId,
            string.Empty, dateTime);
        var densEventsMapWithLmitImageFileName = _mapImageDomainService.GetDensityEventsMapWithLimitImageFileName(
            objectId, string.Empty, dateTime);
        var densEnergyImageFileName = _mapImageDomainService.GetDensityEnergyMapImageFileName(objectId,
            string.Empty, dateTime);

        var result = new Dictionary<DensityMapType, string>()
        {
            [DensityMapType.EventsCount] = densEventsMapImageFileName,
            [DensityMapType.EventsCountWithLimit] = densEventsMapWithLmitImageFileName,
            [DensityMapType.Energy] = densEnergyImageFileName,
        };

        return result;
    }
}
