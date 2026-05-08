using GeoDa.Application.Settings.Repository.Dto;
using GeoDa.Domain.Settings.Models;

namespace GeoDa.Application.Settings.Extensions;

internal static class SettingsExtensions
{
    public static User ToUser(this UserDto userDto)
    {
        var result = new User()
        {
            Name = userDto.Name,
            Password = userDto.Password,
            Role = userDto.Role,
        };

        return result;
    }

    public static UserDto ToUserDto(this User user)
    {
        var result = new UserDto()
        {
            Name = user.Name,
            Password = user.Password,
            Role = user.Role,
        };

        return result;
    }
}
