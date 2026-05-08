using GeoDa.Domain.Settings.Models;
using System;
using System.Collections.Generic;

namespace GeoDa.Application.Settings.Services;

public interface ISettingsService
{
    IReadOnlyList<User> GetAllUsers();

    bool AddUser(User user);

    bool RemoveUser(string userName);

    bool ModifyUser(User user);

    void LogParameterChange(string parameterName, string oldValue, string newValue, string userName, DateTime dt);
}
