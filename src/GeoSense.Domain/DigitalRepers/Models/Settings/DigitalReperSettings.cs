using System.Collections.Generic;

namespace GeoDa.Domain.DigitalRepers.Models.Settings;

public class DigitalReperSettings
{
    public DigitalReperDbSettrings DbSettings { get; set; } = new();

    public Dictionary<string, DigitalReperObjectSettings> ObjectsSettings { get; set; } = new();

    public DigitalReperGeneralSettings GeneralSettings { get; set; } = new();
}
