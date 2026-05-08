using GeoDa.Application.RegionalForecasts.Services.DensityMaps;
using GeoDa.Application.RegionalForecasts.Services.ParamFMaps;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using GeoDa.Domain.RegionalForecasts.Services.BlocksInfoMaps;
using GeoDa.Infrastructure.Services.JsonFiles;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace GeoDa.Application.RegionalForecasts.Services.RfBlocksInfoMaps;

public partial class RfBlocksInfoMapsService : IRfBlocksInfoMapsService
{
    private readonly IRfBlocksInfoMapsDomainService _mapImageDomainService;

    private readonly IJsonFileService _jsonFileService;

    private readonly ILogger<RfBlocksInfoMapsService> _logger;

    public RfBlocksInfoMapsService(
        IRfBlocksInfoMapsDomainService mapImageService,
        IJsonFileService jsonFileService,
        ILogger<RfBlocksInfoMapsService> logger)
    {
        _mapImageDomainService = mapImageService;
        _jsonFileService = jsonFileService;

        _logger = logger;
    }

    // IRfBlocksInfoMapsService
    public bool CreateBlocksInfoMapsImages(
        int objectId,
        DateTime dateTime,
        RegionalForecastObjectSettings settings,
        IReadOnlyList<ParamFBlockInfo> paramFBlockInfo,
        IReadOnlyList<CurrentEnergyBlockInfo> currentEnergyBlockInfo)
    {
        var dirData = Path.Combine("tmp", "maps");
        var webRootImg = Path.Combine("wwwroot", "img");

        // Files with data
        var paramFBlocksInfoDataFileName = _mapImageDomainService.GetParamFBlocksInfoDataFileName(objectId, dirData, 
            dateTime);
        var currentEnergyBlocksInfoDataFileName = _mapImageDomainService.GetCurrentEnergyBlocksInfoDataFileName(
            objectId, dirData, dateTime);

        var mapSettingsFileName = _mapImageDomainService.GetMapBuilderSettingsFileName(objectId, dirData, dateTime);

        // Images file names
        var paramFMapImageFileName = _mapImageDomainService.GetParamFMapImageFileName(objectId, webRootImg, dateTime);
        var totalEnergyMapWithLmitImageFileName = _mapImageDomainService.GetCurrentEnegryImageFileName(objectId,
            webRootImg, dateTime);


        var images = new List<string>
            {
                paramFMapImageFileName,
                totalEnergyMapWithLmitImageFileName
            };

        try
        {
            ClearDataDirectory(objectId, dirData);
            CreateParamFBlocksInfoDataFile(paramFBlocksInfoDataFileName, paramFBlockInfo);
            CreateCurrentEnergyBlocksInfoDataFile(currentEnergyBlocksInfoDataFileName, currentEnergyBlockInfo);

            CreateMapSettingsFile(mapSettingsFileName, images, settings);

            BuildMapsImages(paramFBlocksInfoDataFileName, currentEnergyBlocksInfoDataFileName, mapSettingsFileName);
            RemoveOldImg(objectId, webRootImg, images);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return false;
        }

        return true;
    }

    public Dictionary<BlocksInfoMapType, string> GetBlocksInfoMapsImagesName(int objectId, DateTime dateTime)
    {
        var paramFMapImageFileName = _mapImageDomainService.GetParamFMapImageFileName(objectId, string.Empty, dateTime);
        var currentEnergyMapImageFileName = _mapImageDomainService.GetCurrentEnegryImageFileName(objectId, 
            string.Empty, dateTime);

        var result = new Dictionary<BlocksInfoMapType, string>()
        {
            [BlocksInfoMapType.ParamFMap] = paramFMapImageFileName,
            [BlocksInfoMapType.CurrentEnergyMap] = currentEnergyMapImageFileName,        
        };

        return result;
    }
}
