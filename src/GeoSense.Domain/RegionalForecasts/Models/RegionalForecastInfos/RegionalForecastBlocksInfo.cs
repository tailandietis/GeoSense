using System;

namespace GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;

public class RegionalForecastBlocksInfo
{
    public int ObjectId { get; set; }
    public string ObjectName { get; set; } = string.Empty;
    public DateTime DtOfInfoPreparation { get; set; }
    public RfBlocksAssessment BlocksAssessment { get; set; }
    public BlockInfo[,,] BlocksInfo { get; set; }
}