using System.Collections.Generic;

namespace GeoDa.Domain.SupportPosts.Models.Settings;

public class SupportPostObjectSettings
{
    public string OpcUaServer { get; set; } = string.Empty;

    public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
}
