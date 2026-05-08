using System;
using System.IO;

namespace GeoDa.Domain.RegionalForecasts.Services.BlocksInfoMaps;

internal class RfBlocksInfoMapsDomainService : IRfBlocksInfoMapsDomainService
{
    public string GetParamFBlocksInfoDataFileName(int objectId, string dirData, DateTime dateTime) =>
        Path.Combine(dirData, $"param_f_blocks_info_{objectId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.csv");

    public string GetCurrentEnergyBlocksInfoDataFileName(int objectId, string dirData, DateTime dateTime) =>
        Path.Combine(dirData, $"current_e_info_{objectId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.csv");

    public string GetMapBuilderSettingsFileName(int objectId, string dirData, DateTime dateTime) =>
        Path.Combine(dirData, $"map_settings_bi_{objectId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.json");

    public string GetParamFMapImageFileName(int objectId, string webRootImg, DateTime dateTime) =>
        Path.Combine(webRootImg, $"param_f_map_{objectId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.png");

    public string GetCurrentEnegryImageFileName(int objectId, string webRootImg, DateTime dateTime) =>
        Path.Combine(webRootImg, $"current_e_map_{objectId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.png");
}
