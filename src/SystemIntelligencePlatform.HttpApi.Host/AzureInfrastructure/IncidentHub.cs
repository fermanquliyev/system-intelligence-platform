using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.MultiTenancy;

namespace SystemIntelligencePlatform.AzureInfrastructure;

[Authorize]
public class IncidentHub : Hub
{
    private readonly ICurrentTenant _currentTenant;

    public IncidentHub(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public override async Task OnConnectedAsync()
    {
        var group = _currentTenant.Id.HasValue
            ? $"tenant_{_currentTenant.Id.Value}"
            : "host";

        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = _currentTenant.Id.HasValue
            ? $"tenant_{_currentTenant.Id.Value}"
            : "host";

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        await base.OnDisconnectedAsync(exception);
    }
}
