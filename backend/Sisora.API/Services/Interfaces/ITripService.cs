using Sisora.API.Models.DTOs.Requests;
using Sisora.API.Models.DTOs.Responses;

namespace Sisora.API.Services.Interfaces;

public interface ITripService
{
    Task<ApiResponse<TripResponse>> StartTripAsync(Guid driverId, Guid routeId, StartTripRequest request);
    Task<ApiResponse<TripResponse>> EndTripAsync(Guid driverId, Guid tripId);
    Task<ApiResponse<bool>> UpdateLocationAsync(Guid tripId, UpdateLocationRequest request);
    Task<ApiResponse<TripEventResponse>> RecordTripEventAsync(Guid driverId, Guid tripId, TripEventRequest request);
    Task<ApiResponse<LiveLocationResponse>> GetLiveLocationAsync(Guid tripId);
    Task<ApiResponse<TripResponse>> GetActiveTripAsync(Guid routeId);
    Task<ApiResponse<List<TripResponse>>> GetTripHistoryAsync(Guid routeId);
}