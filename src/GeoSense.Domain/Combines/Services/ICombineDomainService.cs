using GeoDa.Domain.Combines.Models;
using System.Collections.Generic;

namespace GeoDa.Domain.Combines.Services;

public interface ICombineDomainService
{
    bool IsParamExists(string paramName, IReadOnlyList<CombineParamInfo> paramInfos);
}
