using GeoDa.Application.Databases;
using GeoDa.Application.RegionalForecasts.Repository.GsStats.Dtos;

namespace GeoDa.Application.RegionalForecasts.Repository.GsStats;

public interface IGsStatRepository : IRepository
{
    GsStatDto InsertGsStat(GsStatDto gsStat);
}
