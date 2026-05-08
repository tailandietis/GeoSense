using GeoDa.Domain.GeneralForecasts.Models;
using System;

namespace GeoDa.Domain.CurrentForecasts.Models;

public record MassifStateInfo(
    ObjectInfo ObjectInfo,
    DateTime DateTimeReportPrepare,
    MassifStateReport Report,
    MassifStateSettings Settings);