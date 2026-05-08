using System;

namespace GeoDa.Domain.RegionalForecasts.Models;

public record EventsStatistics(
    float MinVal,
    float MaxVal,
    float Q70,
    float Q80,
    float Q90,
    float Q95,
    float Q99,
    int NRows,
    DateTime DateTimeOfOldestEvent,
    DateTime DateTimeOfNewestEvent);
