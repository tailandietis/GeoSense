using GeoDa.Application.Databases;
using GeoDa.Application.GeneralForecasts.Repository.Qualgps.Dtos;
using System;
using System.Collections.Generic;

namespace GeoDa.Application.GeneralForecasts.Repository.Qualgps;

public interface IQualgpRepository : IRepository
{
    List<QualgpDto> SelectAllQualgpsAtTimeRange(int objId, DateTime start, DateTime end);
}
