using GeoDa.Application.Databases;

namespace GeoDa.Database.Postgres;

internal abstract class Repository : IRepository
{
    public string DbConnectionString { get; set; } = string.Empty;
}
