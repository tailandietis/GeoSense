using System.Collections.Generic;

namespace GeoDa.Domain.DigitalRepers.Models.Settings;

public class DigitalReperObjectSettings
{
    public string OpcUaServer { get; set; } = string.Empty;

    public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
}
