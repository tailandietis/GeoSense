using GeoDa.Domain.Models;

namespace GeoDa.Domain.Combines.Models;

public record CombineParameter(
    int Id,
    string Name,
    string Unit,
    float Value,
    Quality Quality);