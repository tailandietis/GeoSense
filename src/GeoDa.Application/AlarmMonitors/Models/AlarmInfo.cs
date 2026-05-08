using GeoDa.Domain.AlarmMonitors.Models;
using System;

namespace GeoDa.Application.AlarmMonitors.Models;

public record AlarmInfo(
    AlarmId AlarmId,
    string ObjectName,
    DateTime DateTime,
    AlarmStatus AlarmStatus);
