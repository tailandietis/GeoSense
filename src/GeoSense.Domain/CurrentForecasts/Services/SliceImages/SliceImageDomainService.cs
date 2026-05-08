using System;
using System.IO;

namespace GeoDa.Domain.CurrentForecasts.Services.SliceImages;

internal class SliceImageDomainService : ISliceImageDomainService
{
    public string GetShtbFileName(int objId, string dirData, DateTime dateTime) =>
        Path.Combine(dirData, $"shtb_{objId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.json");

    public string GetCubeFileName(int objId, string dirData, DateTime dateTime) =>
        Path.Combine(dirData, $"cube_{objId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.json");

    public string GetGeomInfoFileName(int objId, string dirData, DateTime dateTime) =>
        Path.Combine(dirData, $"geom_{objId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.txt");

    public string GetImageFileName(int objId, string webRootImg, DateTime dateTime, bool isMaxLimited = false)
    {
        var limSym = isMaxLimited ? "_L" : string.Empty;

        var result = Path.Combine(webRootImg, $"slice{limSym}_{objId}_{dateTime:yyyy-MM-dd-HH-mm-ss}.png");

        return result;
    }
}
