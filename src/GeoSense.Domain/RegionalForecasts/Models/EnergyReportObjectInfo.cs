using System;

namespace GeoDa.Domain.RegionalForecasts.Models;

public record EnergyReportObjectInfo(
    int ObjectId,
    string ObjectName,
    DateTime DateTimeOfAssessment,
    double EnergyLimit,
    int CheckIntervalInDays,
    double MineMapWidth,
    double MineMapHeight,
    int CountOfBlockAtWidth,
    int CountOfBlockAtHeight);
