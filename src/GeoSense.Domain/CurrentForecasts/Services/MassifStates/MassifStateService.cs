using GeoDa.Domain.CurrentForecasts.Models;
using GeoDa.Domain.Exceptions;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Domain.CurrentForecasts.Services.MassifStates;

internal class MassifStateService : IMassifStateService
{
    private readonly ILogger<MassifStateService> _logger;

    public MassifStateService(ILogger<MassifStateService> logger)
    {
        _logger = logger;
    }

    // IMassifStateService
    public HeterRate GetHeterRate(double dMaxInSlice, MassifStateSettings settings)
    {
        if (dMaxInSlice < settings.DSliceLL)
            return HeterRate.Low;
        else if (dMaxInSlice >= settings.DSliceLL && dMaxInSlice < settings.DSliceHH)
            return HeterRate.Medium;
        else
            return HeterRate.High;
    }

    public bool IsMaxDecompressInBlock(Shtb shtb, List<DfmCubeRow> cube, MassifStateSettings settings)
    {
        (bool isOk, string checkMessage) = IsShtbAndCubeDataOk(shtb, cube);

        if (isOk)
        {
            var block = GetBlockFromSlice(shtb, cube, settings);

            if (block.Any(v => v.Count != 0))
            {
                var DMaxInBlock = block.Select(line => line.Max()).Max();

                if (DMaxInBlock > GetDMaxInSlice(cube) * 0.9)
                    return true;
                else
                    return false;
            }
            else
            {
                var msg = CreateStrMsg("Нет данных в блоке (нулевой размер)", shtb, cube, settings);
                throw new GeoDaDomainException(OpStatus.BadData, msg);
            }
        }
        else
        {
            throw new GeoDaDomainException(OpStatus.BadData, checkMessage);
        }
    }

    public bool IsMinDecompressInBlock(Shtb shtb, List<DfmCubeRow> cube, MassifStateSettings settings)
    {
        (bool isOk, string checkMessage) = IsShtbAndCubeDataOk(shtb, cube);

        if (isOk)
        {
            var block = GetBlockFromSlice(shtb, cube, settings);

            if (block.Any(v => v.Count != 0))
            {
                var heterRate = GetHeterRate(GetDMaxInSlice(cube), settings);
                var DMinInBlock = block.Select(line => line.Min()).Min();

                if (DMinInBlock < settings.DBlockLL && heterRate == HeterRate.High)
                    return true;
                else
                    return false;
            }
            else
            {
                var msg = CreateStrMsg("Нет данных в блоке (нулевой размер)", shtb, cube, settings);
                throw new GeoDaDomainException(OpStatus.BadData, msg);
            }
        }
        else
        {
            throw new GeoDaDomainException(OpStatus.BadData, checkMessage);
        }
    }

    public MassifState GetMassifState(Shtb shtb, List<DfmCubeRow> cube, MassifStateSettings settings)
    {
        (bool isOk, string checkMessage) = IsShtbAndCubeDataOk(shtb, cube);

        if (isOk)
        {
            if (IsMaxDecompressInBlock(shtb, cube, settings))
                return MassifState.Alarm;

            if (IsMinDecompressInBlock(shtb, cube, settings))
                return MassifState.Alarm;

            if (GetHeterRate(GetDMaxInSlice(cube), settings) == HeterRate.High)
                return MassifState.Alarm;

            return MassifState.Normal;
        }
        else
        {
            throw new GeoDaDomainException(OpStatus.BadData, checkMessage);
        }
    }

    public double GetDMaxInSlice(List<DfmCubeRow> cube)
    {
        var result = cube is not null
            ? cube.Select(line => line.Values.Max()).Max()
            : throw new GeoDaDomainException(OpStatus.BadData, "Cube can't be null");

        return result;
    }

    public double GetDMaxInBlock(Shtb shtb, List<DfmCubeRow> cube, MassifStateSettings settings)
    {
        (bool isOk, string checkMessage) = IsShtbAndCubeDataOk(shtb, cube);

        if (isOk)
        {
            var block = GetBlockFromSlice(shtb, cube, settings);

            if (block.Any(v => v.Count != 0))
            {
                return block.Select(line => line.Max()).Max();
            }
            else
            {
                var msg = CreateStrMsg("Нет данных в блоке (нулевой размер)", shtb, cube, settings);
                throw new GeoDaDomainException(OpStatus.BadData, msg);
            }
        }
        else
        {
            throw new GeoDaDomainException(OpStatus.BadData, checkMessage);
        }
    }

    public double GetDMinInBlock(Shtb shtb, List<DfmCubeRow> cube, MassifStateSettings settings)
    {
        (bool isOk, string checkMessage) = IsShtbAndCubeDataOk(shtb, cube);

        if (isOk)
        {
            var block = GetBlockFromSlice(shtb, cube, settings);

            if (block.Any(v => v.Count != 0))
            {
                return block.Select(line => line.Min()).Min();
            }
            else
            {
                var msg = CreateStrMsg("Нет данных в блоке (нулевой размер)", shtb, cube, settings);
                throw new GeoDaDomainException(OpStatus.BadData, msg);
            }
        }
        else
        {
            throw new GeoDaDomainException(OpStatus.BadData, checkMessage);
        }
    }

    // Private

    private static (bool, string) IsShtbAndCubeDataOk(Shtb shtb, List<DfmCubeRow> cube) =>
           shtb is null
           ? (false, $"Args can't be null. Is null Shtb: {shtb is null}")
           : IsCubeDataOk(cube);

    private static (bool, string) IsCubeDataOk(List<DfmCubeRow> cube)
    {
        if (cube is null)
            return (false, $"Args can't be null. Is null Cube: {cube is null}");

        if (cube.Count == 0)
            return (false, $"Cube is empty");

        if (cube.Any(v => v is null))
            return (false, $"Some line(s) in cube is null");

        if (cube.Any(v => v.Values is null))
            return (false, $"Some line(s) in cube has no Values data");

        return (true, "");
    }

    private static List<List<double>> GetBlockFromSlice(Shtb shtb, List<DfmCubeRow> cube, MassifStateSettings settings)
    {
        var block = new List<List<double>>();
        var offsetY = shtb.Y1 / shtb.Dy;
        var offsetX = shtb.X1 / shtb.Dx;

        for (int y = 0; y < cube.Count; y++)
        {
            var posY = offsetY * shtb.Dy + y * shtb.Dy;

            if (posY >= settings.BlockY0 && posY <= settings.BlockY1)
            {
                var row = new List<double>();

                for (int x = 0; x < cube[y].Values.Length; x++)
                {
                    var posX = offsetX * shtb.Dx + x * shtb.Dx;
                    if (posX >= settings.BlockX0 && posX <= settings.BlockX1)
                        row.Add(cube[y].Values[x]);
                }

                block.Add(row);
            }
        }

        return block;
    }

    private static string CreateStrMsg(string msg, Shtb shtb, List<DfmCubeRow> cube, MassifStateSettings settings)
    {
        var strMsg = msg + "\n";
        strMsg += $"Shtb: {shtb}\n";
        strMsg += $"cube: {string.Join("\n", cube.Select(v => v.ToString()))}\n";
        strMsg += $"blockInfo: {settings}\n";
        return strMsg;
    }
}
