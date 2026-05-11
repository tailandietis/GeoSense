using GeoDa.Application.Databases;
using GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies.Dtos;
using System;
using System.Collections.Generic;

namespace GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies;

public interface IGsParamFEnergyRepository : IRepository
{
    void InsertGsParamFEnergy(GsParamFEnergyDto value);
    List<GsParamFEnergyDto> SelectGsParamFEnergy(int objId, DateTime start, DateTime end);
}
