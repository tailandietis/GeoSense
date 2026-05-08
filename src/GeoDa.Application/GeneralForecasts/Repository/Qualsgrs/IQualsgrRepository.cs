using GeoDa.Application.Databases;
using GeoDa.Application.GeneralForecasts.Repository.Qualsgrs.Dtos;
using System;
using System.Collections.Generic;

namespace GeoDa.Application.GeneralForecasts.Repository.Qualsgrs;

public interface IQualsgrRepository : IRepository
{
    List<QualsgrDto> SelectAllQualsgrsAtTimeRange(int objectId, DateTime start, DateTime end);
}
