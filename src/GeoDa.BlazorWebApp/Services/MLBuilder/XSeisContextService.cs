using GeoDa.Application.GeneralForecasts.Services.Utils;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Services.MLBuilder;

public class XSeisMassifStateDto
{
    public int MassifState { get; set; }
    public int HeterRate { get; set; }
    public bool IsMaxDecomp { get; set; }
    public bool IsMinDecomp { get; set; }
    public double DMaxSlice { get; set; }
    public DateTime Dt { get; set; }
    public bool HasData { get; set; }

    public string MassifStateLabel => MassifState switch
    {
        1 => "Признаков опасности не выявлено",
        2 => "Обнаружены признаки опасности",
        _ => "Нет данных"
    };

    public string MassifStateColor => MassifState switch
    {
        1 => "success",
        2 => "error",
        _ => "default"
    };

    public string HeterRateLabel => HeterRate switch
    {
        1 => "Низкая",
        2 => "Умеренная",
        3 => "Высокая",
        _ => "—"
    };
}

public class XSeisContextService
{
    private readonly IGeneralForecastUtilsService _utils;
    private readonly ILogger<XSeisContextService> _logger;

    public XSeisContextService(IGeneralForecastUtilsService utils, ILogger<XSeisContextService> logger)
    {
        _utils = utils;
        _logger = logger;
    }

    public async Task<XSeisMassifStateDto> GetLatestMassifStateAsync(int objId)
    {
        try
        {
            var connStr = _utils.BuildDbConnectionString("DbXSeis");
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            var sql = @"
                SELECT massif_state, heter_rate, is_max_decomp, is_min_decomp, d_max_slice, dt
                FROM xseis.gs_massif_state
                WHERE obj = @obj AND quality = 1
                ORDER BY dt DESC
                LIMIT 1";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("obj", objId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new XSeisMassifStateDto
                {
                    HasData      = true,
                    MassifState  = reader.GetInt32(0),
                    HeterRate    = reader.GetInt32(1),
                    IsMaxDecomp  = reader.GetBoolean(2),
                    IsMinDecomp  = reader.GetBoolean(3),
                    DMaxSlice    = reader.GetDouble(4),
                    Dt           = reader.GetDateTime(5),
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить данные XSeis для obj={ObjId}", objId);
        }

        return new XSeisMassifStateDto { HasData = false };
    }
}
