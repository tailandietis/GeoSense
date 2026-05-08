using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;

public class RfBlocksAssessment
{
    public RfBlocksAssessment(
        RfBlockStatus generalStatus, 
        RfBlockStatus paramfStatus, 
        RfBlockStatus currentEnergyStatus)
    {
        GeneralStatus = generalStatus;
        ParamFStatus = paramfStatus;
        CurrentEnergyStatus = currentEnergyStatus;
    }

    public RfBlockStatus GeneralStatus { get; set; }
    public RfBlockStatus ParamFStatus { get; set; }
    public RfBlockStatus CurrentEnergyStatus { get; set; }
}
