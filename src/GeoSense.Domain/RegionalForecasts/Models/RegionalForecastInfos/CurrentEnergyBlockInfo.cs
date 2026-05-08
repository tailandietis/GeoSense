using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;

public class CurrentEnergyBlockInfo
{
    public CurrentEnergyBlockInfo(double x, double y, double maxCurrentEnergy)
    {
        X = x;
        Y = y;
        MaxCurrentEnergy = maxCurrentEnergy;
    }

    public double X { get; init; }
    public double Y { get; init; }
    public double MaxCurrentEnergy { get; init; }
}
