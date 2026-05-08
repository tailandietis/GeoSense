using GeoDa.Application.Databases;
using GeoDa.Application.GeneralForecasts.Repository.GsParams.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.MsgCodeTexts.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.MsgLogs.Dtos;
using System;
using System.Collections.Generic;

namespace GeoDa.Application.GeneralForecasts.Repository.MsgCodeTexts;

public interface IMsgCodeTextRepository : IRepository
{
    IReadOnlyList<MsgCodeTextDto> SelectAll();
}

