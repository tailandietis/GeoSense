using GeoDa.Application.Databases;
using GeoDa.Application.GeneralForecasts.Repository.GsParams.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.MsgLogs.Dtos;
using System;
using System.Collections.Generic;

namespace GeoDa.Application.GeneralForecasts.Repository.MsgLogs;

public interface IMsgLogRepository : IRepository
{
    IReadOnlyList<MsgLogDto> SelectMessages(int objectId, DateTime start, DateTime end);
}

