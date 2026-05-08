using System.Collections.Generic;

namespace GeoDa.Domain.SupportPosts.Models.Settings;

public class SupportPostSettings
{
    public SupportPostDbSettrings DbSettings { get; set; } = new();

    public Dictionary<string, SupportPostObjectSettings> ObjectsSettings { get; set; } = new();

    public SupportPostGeneralSettings GeneralSettings { get; set; } = new();
}
