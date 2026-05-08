using GeoDa.Domain.SupportPosts.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Domain.SupportPosts.Services;

internal class SupportPostDomainService : ISupportPostDomainService
{
    private readonly ILogger<SupportPostDomainService> _logger;

    public SupportPostDomainService(ILogger<SupportPostDomainService> logger)
    {
        _logger = logger;
    }

    // ISupportPostDomainService
    public bool IsParamExists(string paramName, IReadOnlyList<SupportPostParamInfo> paramInfos)
    {
        var result = paramInfos.Any(v => v.ParamName == paramName);

        return result;
    }
}
