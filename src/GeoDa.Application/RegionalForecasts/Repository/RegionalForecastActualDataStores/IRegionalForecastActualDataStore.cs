using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using System.Threading.Tasks;

namespace GeoDa.Application.RegionalForecasts.Repository.RegionalForecastActualDataStores;

public interface IRegionalForecastActualDataStore
{
    (ObjectStatus, GpnsCountInfo) GetGpnInfo(string objectName);

    void UpdateGpnInfo(string objectName, ObjectStatus objectStatus, GpnsCountInfo gpnsCountInfo);

    (ObjectStatus, EnergyAssessment) GetEnergyAssessment(string objectName);

    void UpdateEnergyAssessment(string objectName, ObjectStatus objectStatus, EnergyAssessment energyAssessment);

    (ObjectStatus, ObjectStatistics) GetObjectStatistics(string objectName);

    void UpdateObjectStatistics(string objectName, ObjectStatus objectStatus, ObjectStatistics objectStat);

    (ObjectStatus Status, RegionalForecastBlocksInfo RfBlocksInfo) GetRfBlocksInfo(string objectName);

    void UpdateRfBlocksInfo(string objectName, ObjectStatus objectStatus, RegionalForecastBlocksInfo rfBlocksInfo);
}
