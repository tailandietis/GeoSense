using GeoDa.Domain.Models;
using System;

namespace GeoDa.Domain.RegionalForecasts.Models;

public record ObjectStatistics(
    int ObjectId,
    DateTime DateTimeOfStatCalc,
    int StatCalcInterval,
    Quality Quality,
    EventsStatistics Statistics);
