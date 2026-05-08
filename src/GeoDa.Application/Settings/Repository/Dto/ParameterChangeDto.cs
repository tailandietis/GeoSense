using System;

namespace GeoDa.Application.Settings.Repository.Dto;

public class ParameterChangeDto
{
    public long Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string ParameterName { get; set; } = string.Empty;

    public string OldValue { get; set; } = string.Empty;

    public string NewValue { get; set; } = string.Empty;

    public DateTime Dt { get; set; }
}
