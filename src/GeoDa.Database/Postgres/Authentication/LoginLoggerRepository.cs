using GeoDa.Application.Authentication.Repository.LoginLoggers;
using GeoDa.Application.Authentication.Repository.LoginLoggers.Dto;
using GeoDa.Application.Exceptions;
using GeoDa.Database.Postgres.Authentication.DbContexts;
using GeoDa.Domain.Authentication.Models;
using GeoDa.Domain.Models;
using Microsoft.Extensions.Logging;
using System;

namespace GeoDa.Database.Postgres.Authentication;

internal class LoginLoggerRepository : Repository, ILoginLoggerRepository
{
    private readonly ILogger<LoginLoggerRepository> _logger;

    public LoginLoggerRepository(ILogger<LoginLoggerRepository> logger)
    {
        _logger = logger;
    }

    // ILoginLoggerRepository
    public void InsertLoginAction(string name, string role, DateTime dt)
    {
        try
        {
            using var dbc = AuthenticationDbContextFactory.Create(DbConnectionString);

            dbc.LoginStamps.Add(new LoginStampDto
            {
                Dt = dt,
                UserName = name,
                Role = role,
                Action = LoginAction.Login.ToString()
            });

            dbc.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: ex.Message);

            throw new GeoDaAppException(OpStatus.InsertDataError, ex.Message);
        }
    }

    public void InsertLogoutAction(string name, string role, DateTime dt)
    {
        try
        {
            using var dbc = AuthenticationDbContextFactory.Create(DbConnectionString);

            dbc.LoginStamps.Add(new LoginStampDto
            {
                Dt = dt,
                UserName = name,
                Role = role,
                Action = LoginAction.Logout.ToString()
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
