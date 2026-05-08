using GeoDa.Application.Databases;
using GeoDa.Application.Settings.Repository.Dto;
using System;
using System.Collections.Generic;

namespace GeodaWebAppEngine.Brokers.SettingsBrokers;

public interface ISettingsRepository : IRepository
{
    List<UserDto> SelectAllUsers();

    void InsertUser(UserDto user);

    void DeleteUser(string userName);

    bool UpdateUser(UserDto user);

    void LogParameterChange(string parameterName, string oldValue, string newValue, string userName, DateTime dt);
}
