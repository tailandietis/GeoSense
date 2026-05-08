using GeoDa.Domain.DigitalRepers.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Domain.DigitalRepers.Services;

internal class DigitalReperDomainService : IDigitalReperDomainService
{
    private readonly ILogger<DigitalReperDomainService> _logger;

    public DigitalReperDomainService(ILogger<DigitalReperDomainService> logger)
    {
        _logger = logger;
    }

    // ICombainDomainService
    public bool IsParamExists(string paramName, IReadOnlyList<DigitalReperParamInfo> paramInfos)
    {
        var result = paramInfos.Any(v => v.ParamName == paramName);

        return result;
    }
}
