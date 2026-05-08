using GeoDa.Application.Databases;
using GeoDa.Application.GeneralForecasts.Repository.GsParams.Dtos;
using System.Collections.Generic;

namespace GeoDa.Application.GeneralForecasts.Repository.GsParams;

public interface IGsParamRepository : IRepository
{
    IReadOnlyList<GsParamDto> InsertGsParams(IReadOnlyList<GsParamDto> gsParams);
}

