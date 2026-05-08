using GeoDa.Domain.CurrentForecasts.Models;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;

namespace GeoDa.Domain.CurrentForecasts.Services.MassifStates;

internal class MassifStateFactory : IMassifStateFactory
{
    private readonly ILogger<MassifStateFactory> _logger;

    public MassifStateFactory(ILogger<MassifStateFactory> logger)
    {
        _logger = logger;
    }

    // IMassifStateFactory
    public MassifStateInfo CreateMassifStateInfo(
       ObjectInfo objectInfo,
       DateTime dateTimeReportPrepare,
       MassifStateReport massifStateReport,
       MassifStateSettings massifStateSettings) =>
        new(objectInfo, dateTimeReportPrepare, massifStateReport, massifStateSettings);

    public MassifStateInfo CreateDefaultMassifStateInfo() =>
        new(ObjectInfo: new(Id: -1, Type: ObjectType.Uncertain, Name: string.Empty),
            DateTimeReportPrepare: DateTime.MinValue,
            Report: CreateDefaultMassifStateReport(),
            Settings: CreateDefaultMassifStateSettings());

    public MassifStateReport CreateMassifStateReport(
        DateTime itemDateTime,
        Quality quality,
        int dostover,
        MassifState massifState,
        HeterRate heterRate,
        bool isMaxDecompressInBlock,
        bool isMinDecompressInBlock,
        double dMaxInSlice,
        double dMaxInBlock,
        double dMinInBlock) =>
        new(ItemDateTime: itemDateTime,
            Quality: quality,
            Dostover: dostover,
            MassifState: massifState,
            HeterRate: heterRate,
            IsMaxDecompressInBlock: isMaxDecompressInBlock,
            IsMinDecompressInBlock: isMinDecompressInBlock,
            DMaxInSlice: dMaxInSlice,
            DMaxInBlock: dMaxInBlock,
            DMinInBlock: dMinInBlock);

    public MassifStateReport CreateDefaultMassifStateReport() =>
        new(DateTime.MinValue, Quality.Uncertain, 0, MassifState.Uncertain, HeterRate.Uncertain, false, false, 0, 0, 0);

    public MassifStateSettings CreateMassifStateSettings(
       double dSliceLL,
       double dSliceHH,
       double dBlockLL,
       double blockX0,
       double blockX1,
       double blockY0,
       double blockY1,
       double maxDValue) =>
        new(DSliceLL: dSliceLL,
            DSliceHH: dSliceHH,
            DBlockLL: dBlockLL,
            BlockX0: blockX0,
            BlockX1: blockX1,
            BlockY0: blockY0,
            BlockY1: blockY1,
            MaxDValue: maxDValue);

    public MassifStateSettings CreateDefaultMassifStateSettings() =>
        new(0, 0, 0, 0, 0, 0, 0, 0);
}
