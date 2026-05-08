using GeoDa.Domain.Models;
using System;

namespace GeoDa.Domain.RegionalForecasts.Models;

public record EnergyReport(
    bool IsAlarm,
    Quality Quality,
    DateTime DateTimeOfItem,
    double Energy);
