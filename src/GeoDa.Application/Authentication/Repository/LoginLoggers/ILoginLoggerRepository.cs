using GeoDa.Application.Databases;
using System;

namespace GeoDa.Application.Authentication.Repository.LoginLoggers;

public interface ILoginLoggerRepository : IRepository
{
    void InsertLoginAction(string name, string role, DateTime dt);

    void InsertLogoutAction(string name, string role, DateTime dt);
}
