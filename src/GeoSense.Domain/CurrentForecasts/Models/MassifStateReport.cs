using GeoDa.Domain.Models;
using System;

namespace GeoDa.Domain.CurrentForecasts.Models;

public record MassifStateReport(
    DateTime ItemDateTime,
    Quality Quality,
    int Dostover,
    MassifState MassifState,
    HeterRate HeterRate,
    bool IsMaxDecompressInBlock,
    bool IsMinDecompressInBlock,
    double DMaxInSlice,
    double DMaxInBlock,
    double DMinInBlock);
