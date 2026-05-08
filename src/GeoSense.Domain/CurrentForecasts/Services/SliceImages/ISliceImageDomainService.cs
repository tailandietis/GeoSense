using System;

namespace GeoDa.Domain.CurrentForecasts.Services.SliceImages;

public interface ISliceImageDomainService
{
    string GetShtbFileName(int objectId, string dirData, DateTime dateTime);

    string GetCubeFileName(int objectId, string dirData, DateTime dateTime);

    string GetGeomInfoFileName(int objectId, string dirData, DateTime dateTime);

    string GetImageFileName(int objectId, string webRootImg, DateTime dateTime, bool isMaxLimited = false);

}
