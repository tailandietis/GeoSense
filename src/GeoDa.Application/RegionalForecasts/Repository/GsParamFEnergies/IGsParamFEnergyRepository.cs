using GeoDa.Application.Databases;
using GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies.Dtos;
using GeoDa.Application.RegionalForecasts.Repository.GsStats.Dtos;
using System.Threading.Tasks;

namespace GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies;

public interface IGsParamFEnergyRepository : IRepository
{
    void InsertGsParamFEnergy(GsParamFEnergyDto value);
}
