using GeoDa.Application.Settings.Repository.Dto;
using GeoDa.Application.Exceptions;
using GeoDa.Database.Postgres;
using GeoDa.Database.Postgres.Settings.DbContexts;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeodaWebAppEngine.Brokers.SettingsBrokers;

internal class SettingsRepository : Repository, ISettingsRepository
{
    private readonly ILogger<SettingsRepository> _logger;

    public SettingsRepository(ILogger<SettingsRepository> logger)
    {
        _logger = logger;
    }

    // ISettingsRepository

    public List<UserDto> SelectAllUsers()
    {
        try
        {
            using var dbc = SettingsDbContextFactory.Create(DbConnectionString);

            var result = dbc.Users.ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.GetDataError, ex.Message);
        }
    }

    public void InsertUser(UserDto user)
    {
        try
        {
            using var dbc = SettingsDbContextFactory.Create(DbConnectionString);

            dbc.Users.Add(user);

            dbc.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.InsertDataError, ex.Message);
        }
    }

    public void DeleteUser(string userName)
    {
        try
        {
            using var dbc = SettingsDbContextFactory.Create(DbConnectionString);

            var user = dbc.Users.Where(v => v.Name == userName).FirstOrDefault();

            if (user != null)
            {
                dbc.Users.Remove(user);
                dbc.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.DeleteDataError, ex.Message);
        }
    }

    public bool UpdateUser(UserDto user)
    {
        try
        {
            using var dbc = SettingsDbContextFactory.Create(DbConnectionString);

            var userToUpdate = dbc.Users.FirstOrDefault(x => x.Name == user.Name);

            if (userToUpdate != null)
            {
                userToUpdate.Password = user.Password;
                userToUpdate.Role = user.Role;
                dbc.SaveChanges();

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.InsertDataError, ex.Message);
        }
    }

    public void LogParameterChange(string parameterName, string oldValue, string newValue, string userName, DateTime dt)
    {
        try
        {
            using var dbc = SettingsDbContextFactory.Create(DbConnectionString);

            dbc.ParameterChanges.Add(new ParameterChangeDto()
            {
                Dt = dt,
                UserName = userName,
                ParameterName = parameterName,
                OldValue = oldValue,
                NewValue = newValue
            });

            dbc.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.InsertDataError, ex.Message);
        }
    }
}
