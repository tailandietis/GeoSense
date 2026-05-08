using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoDa.Domain.RegionalForecasts.Services.Factories;

public interface IRegionalForecastBlocksInfoFactory
{
    public RegionalForecastBlocksInfo Create(
    int objectId,
    string ObjectName,
    DateTime dtOfInfoPreparation,
    RfBlocksAssessment blocksAssessment,
    BlockInfo[,,] blocksInfo);

    public RegionalForecastBlocksInfo CreateDefault();
}
