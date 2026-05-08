using GeoDa.Application.Exceptions;
using GeoDa.Application.RegionalForecasts.Repository.RegionalForecastActualDataStores;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GeoDa.Database.InMemory.RegionalForecasts;

internal class RegionalForecastActualDataStore : IRegionalForecastActualDataStore
{
    private readonly ConcurrentDictionary<string, (ObjectStatus Status, GpnsCountInfo Data)> _systemInfoStore = new();

    private readonly ConcurrentDictionary<string, (ObjectStatus Status, EnergyAssessment Data)> _energyAssessmentStore = new();

    private readonly ConcurrentDictionary<string, (ObjectStatus Status, ObjectStatistics Data)> _objectStatStore = new();

    private readonly ConcurrentDictionary<string, (ObjectStatus Status, RegionalForecastBlocksInfo RfBlocksInfo)> _rfBlockInfoStore =
        new();

    private readonly ILogger<RegionalForecastActualDataStore> _logger;

    public RegionalForecastActualDataStore(ILogger<RegionalForecastActualDataStore> logger)
    {
        _logger = logger;
    }

    // IRegionalForecastActualDataStore
    public (ObjectStatus, GpnsCountInfo) GetGpnInfo(string objectName)
    {
        if (_systemInfoStore.ContainsKey(objectName) is false)
            throw new GeoDaAppException(OpStatus.NoData, $"Object {objectName} is absent in gpn info store");

        return _systemInfoStore[objectName];
    }

    public void UpdateGpnInfo(string objectName, ObjectStatus objectStatus, GpnsCountInfo gpnsCountInfo)
    {
        var result = _systemInfoStore.AddOrUpdate(objectName, (objectStatus, gpnsCountInfo),
           (k, v) => (objectStatus, gpnsCountInfo));

        if (result != (objectStatus, gpnsCountInfo))
            throw new GeoDaAppException(OpStatus.UnkonwnError,
                $"Can't add/update value for {objectName} in gpn info assessment");
    }

    public (ObjectStatus, EnergyAssessment) GetEnergyAssessment(string objectName)
    {
        if (_energyAssessmentStore.ContainsKey(objectName) is false)
            throw new GeoDaAppException(OpStatus.NoData, $"Object {objectName} is absent in energy assessment store");

        return _energyAssessmentStore[objectName];
    }

    public void UpdateEnergyAssessment(string objectName, ObjectStatus objectStatus, EnergyAssessment energyAssessment)
    {
        var result = _energyAssessmentStore.AddOrUpdate(objectName, (objectStatus, energyAssessment),
            (k, v) => (objectStatus, energyAssessment));

        if (result != (objectStatus, energyAssessment))
            throw new GeoDaAppException(OpStatus.UnkonwnError,
                $"Can't add/update value for {objectName} in energy assessment");
    }

    public (ObjectStatus, ObjectStatistics) GetObjectStatistics(string objectName)
    {
        if (_objectStatStore.ContainsKey(objectName) is false)
            throw new GeoDaAppException(OpStatus.NoData, $"Object {objectName} is absent in object stat store");

        return _objectStatStore[objectName];
    }

    public void UpdateObjectStatistics(string objectName, ObjectStatus objectStatus, ObjectStatistics objectStat)
    {
        var result = _objectStatStore.AddOrUpdate(objectName, (objectStatus, objectStat),
            (k, v) => (objectStatus, objectStat));

        if (result != (objectStatus, objectStat))
            throw new GeoDaAppException(OpStatus.UnkonwnError,
                $"Can't add/update value for {objectName} in object stat");
    }

    public (ObjectStatus Status, RegionalForecastBlocksInfo RfBlocksInfo) GetRfBlocksInfo(string objectName)
    {
        if (_rfBlockInfoStore.ContainsKey(objectName) is false)
            throw new GeoDaAppException(OpStatus.NoData, $"Object {objectName} is absent in rf blocks info store");

        return _rfBlockInfoStore[objectName];
    }

    public void UpdateRfBlocksInfo(string objectName,
        ObjectStatus objectStatus,
        RegionalForecastBlocksInfo rfBlocksInfo)
    {
        var result = _rfBlockInfoStore.AddOrUpdate(objectName, (objectStatus, rfBlocksInfo),
            (k, v) => (objectStatus, rfBlocksInfo));

        if (result != (objectStatus, rfBlocksInfo))
            throw new GeoDaAppException(OpStatus.UnkonwnError,
                $"Can't add/update value for {objectName} in rf blocks info store");
    }
}
