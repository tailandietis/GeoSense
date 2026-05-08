using GeoDa.Application.Exceptions;
using GeoDa.Application.GeneralForecasts.Repository.Geometries.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.Geophones.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.MsgLogs.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.ObjectInfos.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.Qualsgrs.Dtos;
using GeoDa.Domain.GeneralForecasts.Models;
using GeoDa.Domain.Models;
using System;

namespace GeoDa.Application.GeneralForecasts.Extensions;

internal static class GeneralForecastExtensions
{
    // ObjectInfo
    public static ObjectInfo ToObjectInfo(this ObjectInfoDto objectInfoDto)
    {
        if (objectInfoDto.ObjTyp >= 0 && objectInfoDto.ObjTyp < Enum.GetValues<ObjectType>().Length)
        {
            var result = new ObjectInfo(Id: objectInfoDto.Obj,
                Name: objectInfoDto.ObjName,
                Type: (ObjectType)objectInfoDto.ObjTyp);

            return result;
        }

        throw new GeoDaAppException(OpStatus.CastModelError, $"Неизвестный тип объекта {objectInfoDto.ObjTyp}");
    }

    // Qualsgr
    public static Qualsgr ToQualsgr(this QualsgrDto qualsgr)
    {
        var result = MapToQualsgr(qualsgr);

        return result;
    }

    public static bool IsSomeProperitiesIsNull(this QualsgrDto qualsgr)
    {
        if (qualsgr.Qc is null
            || qualsgr.Err is null
            || qualsgr.Ngp is null
            || qualsgr.Nsam is null
            || qualsgr.Si is null
            || qualsgr.Condit is null
            || qualsgr.Gain is null
            || qualsgr.Filt is null
            || qualsgr.InclUsed is null
            || qualsgr.Amid is null
            || qualsgr.Amax is null)
            return true;
        else
            return false;
    }

    public static Geometry ToGeometry(this GeometryDto geometryDto)
    {
        var result = new Geometry(ObjectId: geometryDto.Obj,
            NgpMax: geometryDto.NgpMax,
            XMin: geometryDto.XMin,
            XMax: geometryDto.XMax,
            YMin: geometryDto.YMin,
            YMax: geometryDto.YMax,
            WShtrek: geometryDto.WShtrek,
            ZbDir: geometryDto.ZbDir);

        return result;
    }

    public static Geophone ToGeophone(this GeophoneDto geophoneDto)
    {
        var result = new Geophone(ObjectId: geophoneDto.Obj,
            Number: geophoneDto.Num,
            X: geophoneDto.X,
            Y: geophoneDto.Y);

        return result;
    }

    public static MsgLog ToMsgLog(this MsgLogDto msgLogDto, Func<int, int, DateTime> DateTimeConvertor)
    {
        var result = new MsgLog(Id: msgLogDto.ObjectId, 
            DateTime: DateTimeConvertor(msgLogDto.Date, msgLogDto.Time),
            N: msgLogDto.N,
            MsgCode: msgLogDto.MsgCode,
            ErrorCode: msgLogDto.ErrorCode,
            Information: msgLogDto.Information);

        return result;
    }

    // Private
    private static Qualsgr MapToQualsgr(QualsgrDto qualsgr)
    {
        if (qualsgr == null)
            throw new ArgumentNullException(nameof(qualsgr));

        if (qualsgr.IsSomeProperitiesIsNull())
            throw new GeoDaAppException(OpStatus.CastModelError, "One of QualsgrDto's field is null");

        try
        {
            return new()
            {
                Obj = qualsgr.Obj,
                Idat = qualsgr.Idat,
                Itim = qualsgr.Itim,
                Qc = qualsgr.Qc ?? 0,
                Err = qualsgr.Err ?? 0,
                Ngp = qualsgr.Ngp ?? 0,
                Nsam = qualsgr.Nsam ?? 0,
                Si = qualsgr.Si ?? 0,
                Condit = qualsgr.Condit ?? 0,
                Gain = qualsgr.Gain ?? 0,
                Filt = qualsgr.Filt ?? 0,
                InclUsed = qualsgr.InclUsed ?? 0,
                Amid = qualsgr.Amid ?? 0,
                Amax = qualsgr.Amax ?? 0,
            };
        }
        catch (Exception ex)
        {
            throw new GeoDaAppException(OpStatus.CastModelError, ex.Message);
        }
    }
}
