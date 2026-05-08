using AntDesign;
using GeoDa.Application.Authentication.Services;
using GeoDa.Application.Authentication.StateProvider;
using GeoDa.Application.Settings.Services;
using GeoDa.BlazorWebApp.Models.Authentication;
using GeoDa.Domain.Authentication.Models;
using GeoDa.Domain.Services.GeoDaUtils;
using GeoDa.Domain.Settings.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Views.Pages;

public partial class LoginPage
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    [Inject]
    private ILoginLoggerService LoginLoggerService { get; set; } = default!;

    [Inject]
    private IGeoDaDomainUtils Utils { get; set; } = default!;

    [Inject]
    private ILogger<LoginPage> Logger { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private MessageService MessageService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private UserView _model = default!;

    protected override void OnInitialized()
    {
        _model = new UserView();

        base.OnInitialized();
    }

    // Private
    private async void OnFinish(EditContext editContext)
    {
        try
        {
            var users = SettingsService.GetAllUsers();

            User? userAccount = users.Where(u => u.Name == _model.UserName).FirstOrDefault();
            var passHash = Utils.GetStringHash(_model.Password);

            if (userAccount == null || userAccount.Password != passHash)
            {
                await MessageService.Error("Отказ в доступе! Неправильное имя пользователя или пароль", 5);
                return;
            }

            var customAuthStateProvider = (CustomAuthenticationStateProvider)AuthenticationStateProvider;
            await customAuthStateProvider.UpdateAuthenticationStateAsync(new UserSession
            {
                UserName = userAccount.Name,
                Role = userAccount.Role
            });

            LoginLoggerService.Login(await AuthenticationStateTask);

            NavigationManager.NavigateTo("/", true);
        }
        catch (Exception ex)
        {
            Logger.LogError(exception: ex, message: ex.Message);

            await MessageService.Error("Ошибка авторизации. Не удалось установить связь с реестром пользователей.", 5);
        }
    }

    private void OnFinishFailed(EditContext editContext)
    {

    }

    private string GetTitle() => "Вход в систему";
}
