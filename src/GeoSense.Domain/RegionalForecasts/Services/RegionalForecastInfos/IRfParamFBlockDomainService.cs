using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Models.Settings.ParamFBlockSettings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeoDa.Domain.RegionalForecasts.Services.RegionalForecastInfos;

public interface IRfParamFBlockDomainService
{
    Task<BlockInfo[,,]> CalcRfInfoAsync(
        DateTime estimateDt,
        RfFieldSettings fieldSettings,
        RfParamFCalcSettings fCalcSettings,
        RfSeismoActivityEstimationSettings estimationSettings,
        IReadOnlyCollection<Event> seismoEvents);

    IReadOnlyList<ParamFBlockInfo> CreateParamFBlockInfo(RegionalForecastBlocksInfo rfBlocksInfo);

    IReadOnlyList<CurrentEnergyBlockInfo> CreateCurrentEnergyBlockInfo(RegionalForecastBlocksInfo rfBlocksInfo);

    RfBlocksAssessment CalcRfAllBlocksAssessment(BlockInfo[,,] blocksInfo);
}
