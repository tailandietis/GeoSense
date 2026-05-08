namespace GeoDa.Domain.CurrentForecasts.Models;

public record MassifStateSettings(
    double DSliceLL,
    double DSliceHH,
    double DBlockLL,
    double BlockX0,
    double BlockX1,
    double BlockY0,
    double BlockY1,
    double MaxDValue);