namespace GeoDa.Domain.GeneralForecasts.Models;

public record Geometry(
    int ObjectId,
    int NgpMax,
    float XMin,
    float XMax,
    float YMin,
    float YMax,
    float WShtrek,
    int ZbDir);
