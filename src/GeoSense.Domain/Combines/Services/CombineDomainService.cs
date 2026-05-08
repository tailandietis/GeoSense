using GeoDa.Domain.Combines.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Domain.Combines.Services;

internal class CombineDomainService : ICombineDomainService
{
    private readonly ILogger<CombineDomainService> _logger;

    public CombineDomainService(ILogger<CombineDomainService> logger)
    {
        _logger = logger;
    }

    // ICombainDomainService
    public bool IsParamExists(string paramName, IReadOnlyList<CombineParamInfo> paramInfos)
    {
        var result = paramInfos.Any(v => v.ParamName == paramName);

        return result;
    }
}
