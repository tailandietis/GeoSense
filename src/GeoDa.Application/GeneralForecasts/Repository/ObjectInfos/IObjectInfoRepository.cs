using GeoDa.Application.Databases;
using GeoDa.Application.GeneralForecasts.Repository.ObjectInfos.Dtos;
using System.Collections.Generic;

namespace GeoDa.Application.GeneralForecasts.Repository.ObjectInfos;

public interface IObjectInfoRepository : IRepository
{
    List<ObjectInfoDto> SelectAllObjectInfos();
}
