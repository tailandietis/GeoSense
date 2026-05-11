using GeoDa.Application.GeneralForecasts.Repository.ObjectInfos;
using GeoDa.Application.GeneralForecasts.Repository.ObjectInfos.Dtos;
using GeoDa.Application.GeneralForecasts.Services.Utils;
using System.Collections.Generic;

namespace GeoDa.BlazorWebApp.Services.MLBuilder;

public class MLObjectStateService
{
    private readonly IObjectInfoRepository _repo;
    private readonly IGeneralForecastUtilsService _utils;
    private List<ObjectInfoDto>? _cached;

    public int? SelectedObjId { get; set; }
    public string SelectedObjName { get; set; } = "";

    public MLObjectStateService(IObjectInfoRepository repo, IGeneralForecastUtilsService utils)
    {
        _repo = repo;
        _utils = utils;
    }

    public List<ObjectInfoDto> GetObjects()
    {
        if (_cached != null) return _cached;
        _repo.DbConnectionString = _utils.BuildDbConnectionString("DbPeleng");
        _cached = _repo.SelectAllObjectInfos();
        if (_cached.Count > 0 && SelectedObjId is null)
        {
            SelectedObjId = _cached[0].Obj;
            SelectedObjName = _cached[0].ObjName.Trim();
        }
        return _cached;
    }
}
