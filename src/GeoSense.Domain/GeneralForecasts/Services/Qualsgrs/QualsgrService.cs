using GeoDa.Domain.Exceptions;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Domain.GeneralForecasts.Services.Qualsgrs;

internal class QualsgrService : IQualsgrService
{
    private readonly ILogger<QualsgrService> _logger;

    public QualsgrService(ILogger<QualsgrService> logger)
    {
        _logger = logger;
    }

    // IQualsgrService
    public Qualsgr GetNewestQualsgr(IReadOnlyList<Qualsgr> qualsgrs)
    {
        if (qualsgrs is null)
            throw new GeoDaDomainException(OpStatus.BadData, "qualsgrs is null");

        var qualsgr = qualsgrs.OrderByDescending(x => x.Idat)
                            .ThenByDescending(x => x.Itim)
                            .First();

        return qualsgr;
    }

    public double CalcSystemWorkTime(IReadOnlyList<Qualsgr> qualsgrs)
    {
        if (qualsgrs is null)
            throw new GeoDaDomainException(OpStatus.BadData, "qualsgrs is null");

        var result = qualsgrs.Count / 6.0;

        return result;
    }
}
