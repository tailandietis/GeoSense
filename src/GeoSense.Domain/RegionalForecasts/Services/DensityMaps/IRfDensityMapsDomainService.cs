using System;

namespace GeoDa.Domain.RegionalForecasts.Services.BlocksInfoMaps;

public interface IRfDensityMapsDomainService
{
    string GetEventsDataFileName(int objectId, string dirData, DateTime dateTime);

    string GetMapBuilderSettingsFileName(int objectId, string dirData, DateTime dateTime);

    string GetDensityEventsMapImageFileName(int objectId, string webRootImg, DateTime dateTime);

    string GetDensityEventsMapWithLimitImageFileName(int objectId, string webRootImg, DateTime dateTime);

    string GetDensityEnergyMapImageFileName(int objectId, string webRootImg, DateTime dateTime);

}
