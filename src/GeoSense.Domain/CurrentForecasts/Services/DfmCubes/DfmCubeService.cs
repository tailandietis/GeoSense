using GeoDa.Domain.CurrentForecasts.Models;
using Microsoft.Extensions.Logging;

namespace GeoDa.Domain.CurrentForecasts.Services.DfmCubes;

internal class DfmCubeService : IDfmCubeService
{
    private readonly ILogger<DfmCubeService> _logger;

    public DfmCubeService(ILogger<DfmCubeService> logger)
    {
        _logger = logger;
    }

    // IDfmCubeService
    public int DfmCubeRowsCompareByLin(DfmCubeRow x, DfmCubeRow y)
    {
        if (x is null && y is null)
            return 0;
        else if (x is null)
            return -1;
        else if (y is null)
            return 1;
        else
        {
            if (x.Lin > y.Lin)
                return 1;
            else if (x.Lin < y.Lin)
                return -1;
            else
                return 0;
        }
    }
}
