using GeoDa.Domain.AlarmMonitors.Models;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using GeoDa.Domain.RegionalForecasts.Models.RegionalForecastInfos;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeoDa.Application.RegionalForecasts.Services;

public interface IRegionalForecastService
{
    Task<(ObjectStatus, RegionalForecastBlocksInfo)> GetRegionalForecastInfoAsync(
        string objectName, 
        DateTime rfInfoCalcDateTime);

    Task<(ObjectStatus, BlockInfo[,,])> GetRfBlocksInfoAsync(
        string objectName,
        DateTime rfInfoCalcDateTime,
        double blockSize,
        int timeSpanRegInDays,
        int timeSpanMinImpactInDays,
        double backgroundEnergy);

    Dictionary<string, ObjectInfoAndStatus> GetObjectInfoAndStatuses();

    (ObjectStatus, EnergyAssessment) GetEnergyAssessment(string objectName, DateTime dateTime);

    (ObjectStatus, List<Event>) GetLastAlarmEvents(string objectName, DateTime dateTime, int count);

    (ObjectStatus, IReadOnlyList<Event>) GetEvents(string objectName, DateTime start, DateTime end, double minEnergy);

    (ObjectStatus, GpnsCountInfo) GetGpnsCountInfoAtSpecificTime(string objectName, DateTime dateTime);

    AlarmId BuildEnergyAssessmentAlarmId(int objectId);

    AlarmId BuildEnergyAssessmentAlarmId(string objectName);

    (ObjectStatus, ObjectStatistics) GetObjectStatistics(string objectName, DateTime dateTime, int checkInterval);

    (ObjectStatus, ObjectStatistics) GetObjectStatistics(string objectName, DateTime dateTime);

    (ObjectStatus, RegionalForecastBlocksInfo) GetCurrentRfBlocksInfo(string objectName);

    bool BuildDensityMapsImages(EnergyAssessment energyAssessment);

    bool BuildBlocksInfoMapsImages(RegionalForecastBlocksInfo rfBlocksInfo);

    (bool IsOk, string fileName) PreparedFileWithEvents(
        string objectName,
        string pathForStore,
        DateTime start,
        DateTime end,
        double minEnergyValue);

    (bool IsOk, string fileName) PreparedFileWithGpnsCount(
        string objectName,
        string pathForStore,
        DateTime start,
        DateTime end);

    (bool IsOk, string fileName) PreparedFileWithBlocksInfo(string objectName, string pathForStore);

    (bool IsOk, string fileName) PreparedFileWithArchBlocksInfo(string objectName,
        string pathForStore,
        BlockInfo[,,] blockInfos);

    // Qualsgr
    (ObjectStatus, TimeSpan) GetWorkTime(string objectName, DateTime start, DateTime end);

    // MsgLog
    (ObjectStatus, IReadOnlyList<MsgLog>) GetMsgLogs(string objectName, DateTime start, DateTime end);

    string GetMessageText(int messageCode, int errorCode, string additionText);

    RegionalForecastObjectSettings? GetObjectSettings(string objectName);
}
