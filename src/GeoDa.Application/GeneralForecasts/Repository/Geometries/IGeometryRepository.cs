using GeoDa.Application.Databases;
using GeoDa.Application.GeneralForecasts.Repository.Geometries.Dtos;

namespace GeoDa.Application.GeneralForecasts.Repository.Geometries;

public interface IGeometryRepository : IRepository
{
    GeometryDto SelectGeometryData(int objectId);
}
