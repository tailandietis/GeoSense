using GeoDa.Domain.CurrentForecasts.Models;

namespace GeoDa.Domain.CurrentForecasts.Services.DfmCubes;

public interface IDfmCubeService
{
    int DfmCubeRowsCompareByLin(DfmCubeRow x, DfmCubeRow y);
}
