using GeoDa.Application.Authentication.Repository.LoginLoggers;
using GeoDa.Application.Authentication.StateProvider;
using GeoDa.Application.GeneralForecasts.Services.Utils;
using GeoDa.Domain.Models;
using GeoDa.Infrastructure.Services.DateTimes;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace GeoDa.Application.Authentication.Services;

internal class LoginLoggerService : ILoginLoggerService
{
    private readonly string _loginLoggerDbName = "DbGeoDA";

    private readonly ILoginLoggerRepository _loginLoggerRepository;
    private readonly IDateTimeService _dateTimeService;

    private readonly IGeneralForecastUtilsService _gfUtils;

    private readonly ILogger<LoginLoggerService> _logger;


    public LoginLoggerService(
        ILoginLoggerRepository loginLoggerRepository,
        IDateTimeService dateTimeService,
        IGeneralForecastUtilsService rfUtils,
        ILogger<LoginLoggerService> logger)
    {
        _loginLoggerRepository = loginLoggerRepository;
        _dateTimeService = dateTimeService;

        _gfUtils = rfUtils;

        _logger = logger;

        var dbConnString = _gfUtils.BuildDbConnectionString(_loginLoggerDbName);
        _loginLoggerRepository.DbConnectionString = dbConnString;
    }

    // ILoginLoggerService
    public OpStatus Login(AuthenticationState authState)
    {
        var userInfo = CustomAuthenticationStateProvider.GetNameAndRoleFromAuthState(authState);
        var currDt = _dateTimeService.GetCurrentDateTime();

        _loginLoggerRepository.InsertLoginAction(userInfo.Name, userInfo.Role, currDt);

        return OpStatus.Ok;
    }

    public OpStatus Logout(AuthenticationState authState)
    {
        var userInfo = CustomAuthenticationStateProvider.GetNameAndRoleFromAuthState(authState);
        var currDt = _dateTimeService.GetCurrentDateTime();

        _loginLoggerRepository.InsertLogoutAction(userInfo.Name, userInfo.Role, currDt);

        return OpStatus.Ok;
    }
}
