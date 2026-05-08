namespace GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;

public class BlockInfo
{
    public BlockCenterCoords BlockCenterCoords { get; set; } = new BlockCenterCoords();
    public double F { get; set; }
    public long CountOfEvents { get; set; }
    public double TotalEnergy { get; set; }
    public double MaxCurrentEnergy { get; set; }
    public RfBlocksAssessment BlockAssessment { get; set; }
}