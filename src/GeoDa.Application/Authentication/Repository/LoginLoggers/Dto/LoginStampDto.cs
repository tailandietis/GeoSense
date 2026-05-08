using System;

namespace GeoDa.Application.Authentication.Repository.LoginLoggers.Dto;

public class LoginStampDto
{
    public long Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public DateTime Dt { get; set; }
}
