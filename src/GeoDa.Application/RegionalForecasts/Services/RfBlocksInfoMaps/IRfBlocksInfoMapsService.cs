using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoDa.Application.RegionalForecasts.Services.ParamFMaps;

public interface IRfBlocksInfoMapsService
{
    bool CreateBlocksInfoMapsImages(
        int objectId,
        DateTime datetime,
        RegionalForecastObjectSettings settings,
        IReadOnlyList<ParamFBlockInfo> paramFBlockInfo,
        IReadOnlyList<CurrentEnergyBlockInfo> currentEnergyBlockInfo);

    Dictionary<BlocksInfoMapType, string> GetBlocksInfoMapsImagesName(int objectId, DateTime dateTime);
}
