using GeoDa.Domain.CurrentForecasts.Models;
using System.Collections.Generic;

namespace GeoDa.Domain.CurrentForecasts.Services.MassifStates;

public interface IMassifStateService
{
    HeterRate GetHeterRate(double dMaxInSlice, MassifStateSettings settings);

    bool IsMaxDecompressInBlock(Shtb shtb, List<DfmCubeRow> cube, MassifStateSettings settings);

    bool IsMinDecompressInBlock(Shtb shtb, List<DfmCubeRow> cube, MassifStateSettings settings);

    MassifState GetMassifState(Shtb shtb, List<DfmCubeRow> cube, MassifStateSettings settings);

    double GetDMaxInSlice(List<DfmCubeRow> cube);

    double GetDMaxInBlock(Shtb shtb, List<DfmCubeRow> cube, MassifStateSettings settings);

    double GetDMinInBlock(Shtb shtb, List<DfmCubeRow> cube, MassifStateSettings settings);
}
