using System.Collections.Generic;

namespace GeoDa.Application.Databases.Models;

public class DbsSettings
{
    public Dictionary<string, DbConnectionSettings> ConnectionSettings { get; set; } = new();
}
