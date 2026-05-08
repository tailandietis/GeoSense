using System.ComponentModel.DataAnnotations;

namespace GeoDa.BlazorWebApp.Models.Authentication;

public class UserView
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
