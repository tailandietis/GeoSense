using GeoDa.Application.Exceptions;
using GeoDa.Application.GeneralForecasts.Extensions;
using GeoDa.Application.GeneralForecasts.Repository.ObjectInfos;
using GeoDa.Domain.Exceptions;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.GeneralForecasts.Services.Objects;
using GeoDa.Domain.Models;
using GeoDa.Domain.Services.GeoDaUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Application.Objects;

internal class ObjectAppService : IObjectAppService
{
    private readonly IObjectInfoRepository _objectInfoRepository;

    private readonly IObjectService _objectsService;

    private readonly IGeoDaDomainUtils _geoDaDomainUtils;

    private readonly ILogger<ObjectAppService> _logger;

    public ObjectAppService(
        IObjectInfoRepository objectInfoRepository,
        IObjectService objectsService,
        IGeoDaDomainUtils geoDaDomainUtils,
        ILogger<ObjectAppService> logger)
    {
        _objectInfoRepository = objectInfoRepository;
        _objectsService = objectsService;
        _geoDaDomainUtils = geoDaDomainUtils;
        _logger = logger;
    }

    // IObjectAppService
    public void SetDbConnectionString(string dbConnString) =>
        _objectInfoRepository.DbConnectionString = dbConnString;


    public (ObjectStatus Status, int Id) GetObjectStatusAndId(string objectName)
    {
        var objectStatus = ObjectStatus.Uncertain;
        try
        {
            var objectInfos = _objectInfoRepository.SelectAllObjectInfos()
                    .Select(x => x.ToObjectInfo())
                    .ToList()
                    .AsReadOnly();

            objectStatus = _objectsService.GetObjectStatus(objectName, objectInfos);

            if (objectStatus != ObjectStatus.Ok)
                return (objectStatus, -1);

            var objectId = _objectsService.GetObjectIdByName(objectName, objectInfos);

            var result = (objectStatus, objectId);

            return result;
        }
        catch (GeoDaDomainException ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            return (objectStatus, -1);
        }
        catch (GeoDaAppException ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            var status = _geoDaDomainUtils.BuildObjectStatusByOpStatus(ex.OperationStatus);

            return (status, -1);
        }
    }

    public Dictionary<string, ObjectInfoAndStatus> GetObjectInfoAndStatuses(IReadOnlyList<string> objectsFromSettings)
    {
        var objectsInDb = new List<ObjectInfo>();
        var reasonNoDataInDb = ObjectStatus.Absent;

        try
        {
            var tmp = _objectInfoRepository.SelectAllObjectInfos()
                .Select(x => x.ToObjectInfo())
                .ToList();

            objectsInDb.AddRange(tmp);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            reasonNoDataInDb = ObjectStatus.DbError;
        }

        var result = _objectsService.GetObjectInfoAndStatuses(objectsFromSettings, objectsInDb, reasonNoDataInDb);

        return result;
    }
}
