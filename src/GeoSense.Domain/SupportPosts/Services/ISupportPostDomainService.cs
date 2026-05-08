using GeoDa.Domain.SupportPosts.Models;
using System.Collections.Generic;

namespace GeoDa.Domain.SupportPosts.Services;

public interface ISupportPostDomainService
{
    bool IsParamExists(string paramName, IReadOnlyList<SupportPostParamInfo> paramInfos);
}
