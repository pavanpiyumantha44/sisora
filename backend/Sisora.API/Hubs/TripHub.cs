using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Sisora.API.Hubs;

[Authorize]
public class TripHub : Hub
{
    // parent joins a group for a specific trip
    // group name = tripId so only parents on that trip receive updates
    public async Task JoinTripGroup(string tripId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"trip-{tripId}");
    }

    public async Task LeaveTripGroup(string tripId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"trip-{tripId}");
    }
}