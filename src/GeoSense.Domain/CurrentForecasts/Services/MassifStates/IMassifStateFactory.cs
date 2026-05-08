using GeoDa.Domain.CurrentForecasts.Models;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using System;

namespace GeoDa.Domain.CurrentForecasts.Services.MassifStates;

public interface IMassifStateFactory
{
    MassifStateInfo CreateMassifStateInfo(
       ObjectInfo objectInfo,
       DateTime dateTimeReportPrepare,
       MassifStateReport massifStateReport,
       MassifStateSettings massifStateSettings);

    MassifStateInfo CreateDefaultMassifStateInfo();

    MassifStateReport CreateMassifStateReport(
        DateTime itemDatetime,
        Quality quality,
        int dostover,
        MassifState massifState,
        HeterRate heterRate,
        bool isMaxDecompressInBlock,
        bool isMinDecompressInBlock,
        double dMaxInSlice,
        double dMaxInBlock,
        double dMinInBlock);

    MassifStateReport CreateDefaultMassifStateReport();

    MassifStateSettings CreateMassifStateSettings(
       double dSliceLL,
       double dSliceHH,
       double dBlockLL,
       double blockX0,
       double blockX1,
       double blockY0,
       double blockY1,
       double maxDValue);

    MassifStateSettings CreateDefaultMassifStateSettings();
}
