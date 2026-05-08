using GeoDa.Application.GeneralForecasts.Services.Utils;
using GeoDa.Application.Settings.Extensions;
using GeoDa.Domain.Settings.Models;
using GeoDa.Infrastructure.Services.DateTimes;
using GeodaWebAppEngine.Brokers.SettingsBrokers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoDa.Application.Settings.Services;

internal class SettingsService : ISettingsService
{
    private readonly string _settingDbName = "DbGeoDA";

    private readonly ISettingsRepository _settingsRepository;
    private readonly IDateTimeService _dateTimeService;

    private readonly IGeneralForecastUtilsService _gfUtils;

    private readonly ILogger<SettingsService> _logger;


    public SettingsService(
        ISettingsRepository settingsRepository,
        IDateTimeService dateTimeService,
        IGeneralForecastUtilsService rfUtils,
        ILogger<SettingsService> logger)
    {
        _settingsRepository = settingsRepository;
        _dateTimeService = dateTimeService;

        _gfUtils = rfUtils;

        _logger = logger;

        var dbConnString = _gfUtils.BuildDbConnectionString(_settingDbName);
        _settingsRepository.DbConnectionString = dbConnString;
    }

    // ISettingService
    public IReadOnlyList<User> GetAllUsers()
    {
        var result = _settingsRepository.SelectAllUsers()
            .Select(x => x.ToUser())
            .ToList();

        return result;
    }

    public bool AddUser(User user)
    {
        try
        {
            _settingsRepository.InsertUser(user.ToUserDto());

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool RemoveUser(string userName)
    {
        try
        {
            _settingsRepository.DeleteUser(userName);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ModifyUser(User user)
    {
        try
        {
            var result = _settingsRepository.UpdateUser(user.ToUserDto());

            return result;
        }
        catch
        {
            return false;
        }
    }

    public void LogParameterChange(string parameterName, string oldValue, string newValue, string userName, DateTime dt)
    {
        _settingsRepository.LogParameterChange(parameterName, oldValue, newValue, userName, dt);
    }
}
