using Sisora.API.Models.DTOs.Responses;

namespace Sisora.API.Services.Interfaces;

public interface ILocationService
{
    Task SetLiveLocationAsync(Guid tripId, double latitude, double longitude);
    Task<LiveLocationResponse?> GetLiveLocationAsync(Guid tripId);
    Task ClearLiveLocationAsync(Guid tripId);
}