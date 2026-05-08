using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace GeoDa.Domain.RegionalForecasts.Services.Factories;

internal class RegionalForecastBlocksInfoFactory : IRegionalForecastBlocksInfoFactory
{
    public RegionalForecastBlocksInfo Create(
        int objectId,
        string objectName,
        DateTime dtOfInfoPreparation,
        RfBlocksAssessment blocksAssessment,
        BlockInfo[,,] blocksInfo)
    {
        var result = new RegionalForecastBlocksInfo()
        {
            ObjectId = objectId,
            ObjectName = objectName,
            DtOfInfoPreparation = dtOfInfoPreparation,
            BlocksAssessment = blocksAssessment,
            BlocksInfo = blocksInfo
        };

        return result;
    }

    public RegionalForecastBlocksInfo CreateDefault()
    {
        var result = new RegionalForecastBlocksInfo()
        {
            ObjectId = -1,
            ObjectName = string.Empty,
            DtOfInfoPreparation = DateTime.MinValue,
            BlocksAssessment = new(generalStatus: RfBlockStatus.Uncertain, paramfStatus: RfBlockStatus.Uncertain, 
                currentEnergyStatus: RfBlockStatus.Uncertain),
            BlocksInfo = new BlockInfo[0, 0, 0]
        };

        return result;
    }
}
