using GeoDa.Domain.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace GeoDa.Application.Authentication.Services;

public interface ILoginLoggerService
{
    OpStatus Login(AuthenticationState authState);

    OpStatus Logout(AuthenticationState authState);
}
