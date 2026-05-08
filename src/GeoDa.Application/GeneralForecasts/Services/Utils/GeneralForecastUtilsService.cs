using GeoDa.Application.Databases;
using GeoDa.Application.Databases.Models;
using GeoDa.Application.Exceptions;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GeoDa.Application.GeneralForecasts.Services.Utils;

internal class GeneralForecastUtilsService : IGeneralForecastUtilsService
{
    private readonly IDbUtils _dbUtils;

    private readonly DbsSettings _dbsSettings;

    private readonly ILogger<GeneralForecastUtilsService> _logger;

    public GeneralForecastUtilsService(
        IDbUtils dbUtils,
        IOptions<DbsSettings> dbsSettings,
        ILogger<GeneralForecastUtilsService> logger)
    {
        _dbUtils = dbUtils;
        _logger = logger;

        _dbsSettings = dbsSettings.Value;
    }

    //IRegionalForecastServiceUtils
    public string BuildDbConnectionString(string dbName)
    {
        if (_dbsSettings.ConnectionSettings.ContainsKey(dbName) is false)
            throw new GeoDaAppException(OpStatus.ConfigurationError, "Bad config. Db name is absent");

        var dbConn = _dbsSettings.ConnectionSettings[dbName];

        var result = _dbUtils.BuildConnectionString(dbConn.ConnectionString, dbConn.Password);

        return result;
    }
}
