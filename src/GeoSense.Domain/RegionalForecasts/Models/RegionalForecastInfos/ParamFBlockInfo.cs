using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;

public class ParamFBlockInfo
{
    public ParamFBlockInfo(double x, double y, double paramF)
    {
        X = x;
        Y = y;
        ParamF = paramF;
    }

    public double X { get; init; }
    public double Y { get; init; }
    public double ParamF { get; init; }
}
