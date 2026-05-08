using System;
using System.IO;

namespace GeoDa.Domain.RegionalForecasts.Services.BlocksInfoMaps;

internal class RfDensityMapsDomainService : IRfDensityMapsDomainService
{
    public string GetEventsDataFileName(int objectId, string dirData, DateTime dateTime) =>
        Path.Combine(dirData, $"events_{objectId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.csv");

    public string GetMapBuilderSettingsFileName(int objectId, string dirData, DateTime dateTime) =>
        Path.Combine(dirData, $"map_settings_{objectId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.json");

    public string GetDensityEventsMapImageFileName(int objectId, string webRootImg, DateTime dateTime) =>
        Path.Combine(webRootImg, $"events_map_{objectId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.png");

    public string GetDensityEventsMapWithLimitImageFileName(int objectId, string webRootImg, DateTime dateTime) =>
        Path.Combine(webRootImg, $"events_map_limit_{objectId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.png");

    public string GetDensityEnergyMapImageFileName(int objectId, string webRootImg, DateTime dateTime) =>
        Path.Combine(webRootImg, $"energy_map_limit_{objectId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.png");
}
