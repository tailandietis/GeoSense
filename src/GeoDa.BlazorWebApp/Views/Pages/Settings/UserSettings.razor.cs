using AntDesign;
using GeoDa.Application.Authentication.StateProvider;
using GeoDa.Application.Settings.Services;
using GeoDa.BlazorWebApp.Models.Settings.Users;
using GeoDa.Domain.Services.GeoDaUtils;
using GeoDa.Domain.Settings.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Views.Pages.Settings;

enum Mode { UNKNOWN, CONFIG, ADD };

public partial class UserSettings
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    [Inject]
    private IGeoDaDomainUtils Utils { get; set; } = default!;

    [Inject]
    private MessageService MessageService { get; set; } = default!;

    [Inject]
    private ILogger<UserSettings> Logger { get; set; } = default!;

    private IEnumerable<User>? _selectedRows;

    private IEnumerable<User>? SelectedRows
    {
        get => _selectedRows;
        set
        {
            _selectedRows = value;
            _isConfigButtonDisable = value == null;
        }
    }

    private readonly List<User> _usersTable = new();

    private User _selectedUser = default!;

    private UserSettingsView _userSettings = new();

    private Form<UserSettingsView> _form = default!;

    private readonly List<CascaderNode> _roleNames = new();


    private string _modalTitle = string.Empty;

    private bool _isModalVisible = false;

    private bool _isUserNameDisable = false;

    private bool _isConfigButtonDisable = true;

    private Mode _mode = Mode.UNKNOWN;

    protected override void OnInitialized()
    {
        LoadUsers();

        _roleNames.Clear();
        _roleNames.AddRange(Role.Names.Select(v => new CascaderNode { Value = v, Label = v }).ToList());

        base.OnInitialized();
    }

    // Events handle
    // Main window
    private readonly User _oldUser = new();

    private void OnConfigUser()
    {
        _modalTitle = "Настройка пользователя";

        if (SelectedRows?.Count() == 1)
        {
            _selectedUser = SelectedRows.First();
            _userSettings.Name = _selectedUser.Name;
            _userSettings.Password = "";
            _userSettings.ConfirmPassword = "";
            _userSettings.Role = _selectedUser.Role;

            _oldUser.Name = _selectedUser.Name;
            _oldUser.Role = _selectedUser.Role;
            _oldUser.Password = _selectedUser.Password;

            _mode = Mode.CONFIG;

            _isUserNameDisable = true;
            _isModalVisible = true;
        }
    }

    private void OnAddUser()
    {
        _modalTitle = "Добавление пользователя";

        _mode = Mode.ADD;

        _isModalVisible = true;
    }

    private void OnDeleteUser()
    {
        if (SelectedRows?.Count() == 1)
        {
            _selectedUser = SelectedRows.First();

            DeleteUserProc();

            SelectedRows = null;
        }
    }

    // Add / Config dialog
    private void OnModalOk()
    {
        _form.Submit();
    }

    private void OnModalCancel()
    {
        _isModalVisible = false;
    }

    private void OnFormFinish(EditContext editContext)
    {
        if (_userSettings.Password != _userSettings.ConfirmPassword)
        {
            MessageService.Error("Пароли не совпадают");
        }
        else
        {
            if (_mode == Mode.CONFIG)
            {
                ChangeUserProc();
                _isModalVisible = false;
            }
            else if (_mode == Mode.ADD)
            {
                if (AddUserProc())
                    _isModalVisible = false;
            }
        }

        _isUserNameDisable = false;
    }

    private void OnFormFinishFailed()
    {

    }

    // Private
    private async void LogUserChangesAsync(User oldUser, User newUser)
    {
        var userInfo = CustomAuthenticationStateProvider.GetNameAndRoleFromAuthState(await AuthenticationStateTask);

        if (oldUser.Name != newUser.Name)
            SettingsService.LogParameterChange("User name changed", _oldUser.Name, _userSettings.Name,
                userInfo.Name, DateTime.Now);

        if (oldUser.Role != newUser.Role)
            SettingsService.LogParameterChange("User role changed", _oldUser.Role, _userSettings.Role,
                userInfo.Name, DateTime.Now);

        if (oldUser.Password != newUser.Password)
            SettingsService.LogParameterChange("User password changed", "", "", userInfo.Name, DateTime.Now);
    }

    private async void LogUserAddAsync(User user)
    {
        var userInfo = CustomAuthenticationStateProvider.GetNameAndRoleFromAuthState(await AuthenticationStateTask);

        SettingsService.LogParameterChange($"Add user name: {user.Name}, role: {user.Role}", "", "",
            userInfo.Name, DateTime.Now);
    }

    private async void LogUserDeleteAsync(User user)
    {
        var userInfo = CustomAuthenticationStateProvider.GetNameAndRoleFromAuthState(await AuthenticationStateTask);

        SettingsService.LogParameterChange($"Delete user name: {user.Name}, role: {user.Role}", "", "",
            userInfo.Name, DateTime.Now);
    }

    private void ChangeUserProc()
    {
        var selUser = _usersTable.Where(v => v.Name == _selectedUser.Name && v.Role == _selectedUser.Role)
                            .FirstOrDefault();
        if (selUser != null)
        {
            var user = new User()
            {
                Name = _userSettings.Name,
                Role = _userSettings.Role,
                Password = Utils.GetStringHash(_userSettings.Password),
            };

            var isOk = SettingsService.ModifyUser(user);

            if (isOk)
            {
                selUser.Name = _userSettings.Name;
                selUser.Role = _userSettings.Role;

                LogUserChangesAsync(_oldUser, user);
            }
            else
            {
                MessageService.Error("Ошибка сохранения профиля пользователя в реестре");
            }
        }
        else
        {
            MessageService.Error("Ошибка сохранения профиля пользователя");
        }
    }

    private bool AddUserProc()
    {
        if (_usersTable.Any(v => v.Name == _userSettings.Name))
        {
            MessageService.Error("Пользователь с указанным именем уже есть в системе!");
            return false;
        }
        else
        {
            var user = new User()
            {
                Name = _userSettings.Name,
                Role = _userSettings.Role,
                Password = Utils.GetStringHash(_userSettings.Password),
            };

            var isOk = SettingsService.AddUser(user);

            if (isOk)
            {
                _usersTable.Add(new() { Name = _userSettings.Name, Role = _userSettings.Role });
                LogUserAddAsync(user);
            }
            else
            {
                MessageService.Error("Ошибка сохранения профиля пользователя в реестре");
            }
        }

        return true;
    }

    private void DeleteUserProc()
    {
        var isOk = SettingsService.RemoveUser(_selectedUser.Name);

        if (isOk)
        {
            var usersForRemove = _usersTable
                                    .Where(v => v.Name == _selectedUser.Name && v.Role == _selectedUser.Role)
                                    .FirstOrDefault();

            if (usersForRemove != null)
                _usersTable.Remove(usersForRemove);

            LogUserDeleteAsync(_selectedUser);
        }
    }

    private void LoadUsers()
    {
        _usersTable.Clear();

        try
        {
            var users = SettingsService.GetAllUsers();

            _usersTable.AddRange(users);
        }
        catch (Exception ex)
        {
            Logger.LogError(exception: ex, message: ex.Message);
        }
    }

    private string GetTitle() =>
        "Пользователи";

    private string GetModalTitle() =>
        _modalTitle;
}
