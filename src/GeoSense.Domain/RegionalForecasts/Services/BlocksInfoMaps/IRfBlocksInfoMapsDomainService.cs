using System;

namespace GeoDa.Domain.RegionalForecasts.Services.BlocksInfoMaps;

public interface IRfBlocksInfoMapsDomainService
{
    string GetParamFBlocksInfoDataFileName(int objectId, string dirData, DateTime dateTime);

    string GetCurrentEnergyBlocksInfoDataFileName(int objectId, string dirData, DateTime dateTime);

    string GetMapBuilderSettingsFileName(int objectId, string dirData, DateTime dateTime);

    string GetParamFMapImageFileName(int objectId, string webRootImg, DateTime dateTime);

    string GetCurrentEnegryImageFileName(int objectId, string webRootImg, DateTime dateTime);

}
