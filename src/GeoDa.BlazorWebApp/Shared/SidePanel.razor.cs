using AntDesign;
using GeoDa.Application.AlarmMonitors.Services.AlarmMonitorServices;
using GeoDa.Application.Authentication.Services;
using GeoDa.Application.Authentication.StateProvider;
using GeoDa.BlazorWebApp.Services.Observers;
using GeoDa.Domain.AlarmMonitors.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.Shared;

public partial class SidePanel : IObserverClient, IDisposable
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    [Inject]
    private ILoginLoggerService LoginLoggerService { get; set; } = default!;

    [Inject]
    private IObserverService ObserverService { get; set; } = default!;

    [Inject]
    private IAlarmMonitorService AlarmMonitorService { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private NotificationService Notice { get; set; } = default!;

    private bool _isAlarm;

    private readonly List<AlarmId> _showedAlarms = new();

    protected override void OnInitialized()
    {
        if (!ObserverService.IsRegistered(this))
            ObserverService.RegisterObserver(this);

        base.OnInitialized();
    }

    // IObserverClient
    public void Update(object data)
    {
        var oldAlarmState = _isAlarm;
        _isAlarm = AlarmMonitorService.IsAnyAlarmOccured();

        NotificationProcessing();

        if (oldAlarmState != _isAlarm)
            InvokeAsync(() => StateHasChanged());
    }

    // IDisposable
    public void Dispose() =>
        ObserverService.RemoveObserver(this);

    // Private
    private async void Logout(MouseEventArgs mouseEventArgs)
    {
        var customAuthStateProvider = (CustomAuthenticationStateProvider)AuthenticationStateProvider;

        //var userInfo = await GetNameAndRoleFromCurrentSession();            
        //LoginLoggerService.Logout(userInfo.Name, userInfo.Role, DateTime.Now);

        LoginLoggerService.Logout(await AuthenticationStateTask);

        await customAuthStateProvider.UpdateAuthenticationStateAsync(null);

        NavigationManager.NavigateTo("/", true);
    }

    private async void NotificationProcessing()
    {
        var user = (await AuthenticationStateTask).User;

        if (user.Identity != null && user.Identity.IsAuthenticated)
        {
            var activeAlarms = AlarmMonitorService.GetAllActiveAlarms()
                                .Where(v => v.AlarmStatus == AlarmStatus.Active)
                                .ToList();

            foreach (var alarm in activeAlarms)
            {
                if (!_showedAlarms.Where(v => v.Equals(alarm.AlarmId)).Any())
                {
                    var config = new NotificationConfig()
                    {
                        Message = "ВНИМАНИЕ!",
                        Duration = 0,
                        Description = AlarmMonitorService.GetTextDescription(alarm.AlarmId),
                        Style = "background-color:lightcoral",
                    };
                    config.OnClose += () => CloseNotification(alarm.AlarmId);

                    await Notice.Open(config);

                    _showedAlarms.Add(alarm.AlarmId);
                }
            }
        }
    }

    private void CloseNotification(AlarmId alarmId)
    {
        try
        {
            AlarmMonitorService.SetAlarmStatus(alarmId, AlarmStatus.Confirmed);
        }
        catch
        {

        }
        _showedAlarms.RemoveAll(v => v.Equals(alarmId));
    }
}
