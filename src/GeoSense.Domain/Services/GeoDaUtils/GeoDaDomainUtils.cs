using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;

namespace GeoDa.Domain.Services.GeoDaUtils;

internal class GeoDaDomainUtils : IGeoDaDomainUtils
{
    private const int HourOffset = 10000;
    private const int MinuteOffset = 100;

    private readonly ILogger<GeoDaDomainUtils> _logger;

    public GeoDaDomainUtils(ILogger<GeoDaDomainUtils> logger)
    {
        _logger = logger;
    }

    // IGeoDaDomainUtils
    public int GetHourOffset() =>
        HourOffset;

    public (DateTime startDay, DateTime endDay) CreateDateInterval(int intervalInDays, DateTime referenceDateTime)
    {
        var result = (referenceDateTime.AddDays(-1 * intervalInDays), referenceDateTime);

        return result;
    }

    public DateTime ConvertAzonFormatToDateTime(int date, int time)
    {
        var dt = ConvertAzonFormatToDateTime(date);

        if (time < 0)
            throw new ArgumentException($"Time must be >= 0, time: {time}");

        return new DateTime(dt.Year, dt.Month, dt.Day, time / 10000, time / 100 % 100, time % 100);
    }

    public DateTime ConvertAzonFormatToDateTime(int date) =>
        date switch
        {
            int when date <= 0 =>
                throw new ArgumentException($"Date must be > 0: date: {date}"),
            int when date / 100 % 100 == 0 =>
                throw new ArgumentException($"Date month must be > 0: date: {date}"),
            int when date % 100 == 0 => throw
                new ArgumentException($"Date day must be > 0: date: {date}"),
            _ => new(2000 + date / 10000,
                    date / 100 % 100,
                    date % 100),
        };

    public int GetTimeInAzonFormat(DateTime dt) =>
        dt.TimeOfDay.Hours * 10000
        + dt.TimeOfDay.Minutes * 100
        + dt.TimeOfDay.Seconds;

    public int GetDateInAzonFormat(DateTime dt) =>
        dt.Date.Year % 100 * 10000
        + dt.Date.Month * 100
        + dt.Date.Day;

    public string GetStringHash(string str)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] sourceBytes = Encoding.UTF8.GetBytes(str);
        byte[] hashBytes = sha256.ComputeHash(sourceBytes);

        string hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

        return hash;
    }

    public ObjectStatus BuildObjectStatusByOpStatus(OpStatus opStatus) =>
        opStatus switch
        {
            OpStatus.GetDataError => ObjectStatus.DbError,
            OpStatus.InsertDataError => ObjectStatus.DbError,
            _ => ObjectStatus.Uncertain
        };
}
