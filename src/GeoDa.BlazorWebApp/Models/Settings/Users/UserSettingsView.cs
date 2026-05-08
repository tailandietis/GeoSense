using System.ComponentModel.DataAnnotations;

namespace GeoDa.BlazorWebApp.Models.Settings.Users;

public class UserSettingsView
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
