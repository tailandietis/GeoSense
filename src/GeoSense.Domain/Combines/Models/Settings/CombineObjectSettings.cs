using System.Collections.Generic;

namespace GeoDa.Domain.Combines.Models.Settings;

public class CombineObjectSettings
{
    public string OpcUaServer { get; set; } = string.Empty;

    public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
}
