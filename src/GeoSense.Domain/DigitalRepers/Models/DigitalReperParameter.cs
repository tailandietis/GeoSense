using GeoDa.Domain.Models;

namespace GeoDa.Domain.DigitalRepers.Models;

public record DigitalReperParameter(
    int Id,
    string Name,
    string Unit,
    float Value,
    Quality Quality);