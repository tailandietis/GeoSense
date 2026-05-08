using GeoDa.Domain.DigitalRepers.Models;
using System.Collections.Generic;

namespace GeoDa.Domain.DigitalRepers.Services;

public interface IDigitalReperDomainService
{
    bool IsParamExists(string paramName, IReadOnlyList<DigitalReperParamInfo> paramInfos);
}
