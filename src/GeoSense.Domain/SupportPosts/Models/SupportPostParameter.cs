using GeoDa.Domain.Models;

namespace GeoDa.Domain.SupportPosts.Models;

public record SupportPostParameter(
    int Id,
    string Name,
    string Unit,
    float Value,
    Quality Quality);