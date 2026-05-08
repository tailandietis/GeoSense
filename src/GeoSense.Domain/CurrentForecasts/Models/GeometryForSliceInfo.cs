using System.Collections.Generic;

namespace GeoDa.Domain.CurrentForecasts.Models;

public record GeometryForSliceInfo(
    string CountOfGeophones,
    Dictionary<string, string> Coords,
    string X,
    string Y,
    string Direction);
