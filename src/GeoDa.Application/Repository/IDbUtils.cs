namespace GeoDa.Application.Databases;

public interface IDbUtils
{
    string BuildConnectionString(string connectionString, string password);
}
