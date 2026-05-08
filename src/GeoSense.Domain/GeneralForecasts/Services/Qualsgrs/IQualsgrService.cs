using GeoDa.Domain.GeneralForecasts.Models;
using System.Collections.Generic;

namespace GeoDa.Domain.GeneralForecasts.Services.Qualsgrs;

public interface IQualsgrService
{
    Qualsgr GetNewestQualsgr(IReadOnlyList<Qualsgr> qualsgrs);

    double CalcSystemWorkTime(IReadOnlyList<Qualsgr> qualsgrs);
}
