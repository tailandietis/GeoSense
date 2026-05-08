using GeoDa.Application.Databases;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace GeoDa.Database.Postgres;

internal class DbUtils : IDbUtils
{
    private const string DataProtectionApplicationName = "GeoDA Application";
    private const string PurposeString = "Password protection";

    private readonly ILogger<DbUtils> _logger;

    public DbUtils(ILogger<DbUtils> logger)
    {
        _logger = logger;
    }

    public string BuildConnectionString(string connectionString, string password)
    {
        var unprotectPassword = Unprotect(password);

        var result = connectionString + ";Password=" + unprotectPassword;

        return result;
    }

    // Private
    private string Unprotect(string text)
    {
        var dataProtectionProvider = DataProtectionProvider.Create(DataProtectionApplicationName);
        var protector = dataProtectionProvider.CreateProtector(PurposeString);
        var unprotectedText = protector.Unprotect(text);

        return unprotectedText;
    }
}
