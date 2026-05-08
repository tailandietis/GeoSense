using System.Collections.Generic;

namespace GeoDa.Domain.Combines.Models.Settings;

public class CombineSettings
{
    public CombineDbSettrings DbSettings { get; set; } = new();

    public Dictionary<string, CombineObjectSettings> ObjectsSettings { get; set; } = new();

    public CombineGeneralSettings GeneralSettings { get; set; } = new();
}
