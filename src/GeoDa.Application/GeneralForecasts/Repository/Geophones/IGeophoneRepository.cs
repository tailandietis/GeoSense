using GeoDa.Application.Databases;
using GeoDa.Application.GeneralForecasts.Repository.Geophones.Dtos;
using System.Collections.Generic;

namespace GeoDa.Application.GeneralForecasts.Repository.Geophones;

public interface IGeophoneRepository : IRepository
{
    List<GeophoneDto> SelectAllGeophonsData(int objId);
}
