using backend.Models.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sisora.API.Data;
using Sisora.API.Hubs;
using Sisora.API.Models.DTOs.Requests;
using Sisora.API.Models.DTOs.Responses;
using Sisora.API.Models.Entities;
using Sisora.API.Models.Enums;
using Sisora.API.Services.Interfaces;

namespace Sisora.API.Services;

public class TripService : ITripService
{
    private readonly AppDbContext _context;
    private readonly ILocationService _locationService;
    private readonly IHubContext<TripHub> _hubContext;

    public TripService(
        AppDbContext context,
        ILocationService locationService,
        IHubContext<TripHub> hubContext)
    {
        _context = context;
        _locationService = locationService;
        _hubContext = hubContext;
    }

    // ── Start Trip ────────────────────────────────────────
    public async Task<ApiResponse<TripResponse>> StartTripAsync(Guid driverId, Guid routeId, StartTripRequest request)
    {
        var route = await _context.ServiceRoutes
            .Include(r => r.Driver)
            .FirstOrDefaultAsync(r => r.Id == routeId && r.DriverId == driverId && r.IsActive);

        if (route == null)
            return ApiResponse<TripResponse>.Fail("Route not found.");

        if (route.Driver.Status != DriverStatus.Approved)
            return ApiResponse<TripResponse>.Fail("Driver account is not approved.");

        // check no active trip already running on this route
        var activeTrip = await _context.Trips
            .AnyAsync(t => t.ServiceRouteId == routeId && t.Status == TripStatus.Active);

        if (activeTrip)
            return ApiResponse<TripResponse>.Fail("A trip is already active on this route.");

        if (!Enum.TryParse<TripType>(request.TripType, ignoreCase: true, out var tripType))
            return ApiResponse<TripResponse>.Fail("Invalid trip type. Use Morning or Afternoon.");

        var trip = new Trip
        {
            ServiceRouteId = routeId,
            TripType = tripType,
            Status = TripStatus.Active,
            StartedAt = DateTime.UtcNow
        };

        await _context.Trips.AddAsync(trip);
        await _context.SaveChangesAsync();

        // notify all parents on this route via SignalR
        await _hubContext.Clients
            .Group($"trip-{trip.Id}")
            .SendAsync("TripStarted", new
            {
                TripId = trip.Id,
                RouteName = route.Name,
                StartedAt = trip.StartedAt
            });

        return ApiResponse<TripResponse>.Ok(MapToTripResponse(trip, route.Name, new List<TripEventResponse>()),
            "Trip started successfully.");
    }

    // ── End Trip ──────────────────────────────────────────
    public async Task<ApiResponse<TripResponse>> EndTripAsync(Guid driverId, Guid tripId)
    {
        var trip = await _context.Trips
            .Include(t => t.ServiceRoute)
            .Include(t => t.TripEvents)
                .ThenInclude(te => te.Student)
            .FirstOrDefaultAsync(t => t.Id == tripId
                && t.ServiceRoute.DriverId == driverId
                && t.Status == TripStatus.Active);

        if (trip == null)
            return ApiResponse<TripResponse>.Fail("Active trip not found.");

        trip.Status = TripStatus.Completed;
        trip.EndedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // clear live location from Redis
        await _locationService.ClearLiveLocationAsync(tripId);

        // notify parents trip ended
        await _hubContext.Clients
            .Group($"trip-{tripId}")
            .SendAsync("TripEnded", new
            {
                TripId = tripId,
                EndedAt = trip.EndedAt
            });

        return ApiResponse<TripResponse>.Ok(
            MapToTripResponse(trip, trip.ServiceRoute.Name, MapToEventResponses(trip.TripEvents)),
            "Trip ended successfully.");
    }

    // ── Update Location ───────────────────────────────────
    public async Task<ApiResponse<bool>> UpdateLocationAsync(Guid tripId, UpdateLocationRequest request)
    {
        var trip = await _context.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId && t.Status == TripStatus.Active);

        if (trip == null)
            return ApiResponse<bool>.Fail("Active trip not found.");

        // store in Redis
        await _locationService.SetLiveLocationAsync(tripId, request.Latitude, request.Longitude);

        // broadcast to all parents watching this trip
        await _hubContext.Clients
            .Group($"trip-{tripId}")
            .SendAsync("LocationUpdated", new
            {
                TripId = tripId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Timestamp = DateTime.UtcNow
            });

        return ApiResponse<bool>.Ok(true);
    }

    // ── Record Trip Event (pickup / dropoff) ──────────────
    public async Task<ApiResponse<TripEventResponse>> RecordTripEventAsync(Guid driverId, Guid tripId, TripEventRequest request)
    {
        var trip = await _context.Trips
            .Include(t => t.ServiceRoute)
            .FirstOrDefaultAsync(t => t.Id == tripId
                && t.ServiceRoute.DriverId == driverId
                && t.Status == TripStatus.Active);

        if (trip == null)
            return ApiResponse<TripEventResponse>.Fail("Active trip not found.");

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == request.StudentId
                && s.ServiceRouteId == trip.ServiceRouteId
                && s.IsActive);

        if (student == null)
            return ApiResponse<TripEventResponse>.Fail("Student not found on this route.");

        // determine event type based on trip type
        // morning trip = picking up, afternoon = dropping off
        var eventType = trip.TripType == TripType.Morning
            ? TripEventType.PickedUp
            : TripEventType.DroppedOff;

        var tripEvent = new TripEvent
        {
            TripId = tripId,
            StudentId = student.Id,
            EventType = eventType,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Timestamp = DateTime.UtcNow
        };

        await _context.TripEvents.AddAsync(tripEvent);
        await _context.SaveChangesAsync();

        var eventResponse = new TripEventResponse
        {
            StudentId = student.Id,
            StudentName = student.FullName,
            EventType = eventType.ToString(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Timestamp = tripEvent.Timestamp
        };

        // notify the specific student's parents
        var parentIds = await _context.ParentStudents
            .Where(ps => ps.StudentId == student.Id)
            .Select(ps => ps.ParentId.ToString())
            .ToListAsync();

        await _hubContext.Clients
            .Group($"trip-{tripId}")
            .SendAsync("StudentEventRecorded", new
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                EventType = eventType.ToString(),
                Timestamp = tripEvent.Timestamp
            });

        return ApiResponse<TripEventResponse>.Ok(eventResponse,
            $"{student.FullName} marked as {eventType}.");
    }

    // ── Get Live Location ─────────────────────────────────
    public async Task<ApiResponse<LiveLocationResponse>> GetLiveLocationAsync(Guid tripId)
    {
        var trip = await _context.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip == null)
            return ApiResponse<LiveLocationResponse>.Fail("Trip not found.");

        var location = await _locationService.GetLiveLocationAsync(tripId);

        if (location == null)
            return ApiResponse<LiveLocationResponse>.Fail("No location data available.");

        return ApiResponse<LiveLocationResponse>.Ok(location);
    }

    // ── Get Active Trip ───────────────────────────────────
    public async Task<ApiResponse<TripResponse>> GetActiveTripAsync(Guid routeId)
    {
        var trip = await _context.Trips
            .Include(t => t.ServiceRoute)
            .Include(t => t.TripEvents)
                .ThenInclude(te => te.Student)
            .FirstOrDefaultAsync(t => t.ServiceRouteId == routeId && t.Status == TripStatus.Active);

        if (trip == null)
            return ApiResponse<TripResponse>.Fail("No active trip on this route.");

        return ApiResponse<TripResponse>.Ok(
            MapToTripResponse(trip, trip.ServiceRoute.Name, MapToEventResponses(trip.TripEvents)));
    }

    // ── Get Trip History ──────────────────────────────────
    public async Task<ApiResponse<List<TripResponse>>> GetTripHistoryAsync(Guid routeId)
    {
        var trips = await _context.Trips
            .Include(t => t.ServiceRoute)
            .Include(t => t.TripEvents)
                .ThenInclude(te => te.Student)
            .Where(t => t.ServiceRouteId == routeId && t.Status == TripStatus.Completed)
            .OrderByDescending(t => t.StartedAt)
            .Take(30)
            .ToListAsync();

        return ApiResponse<List<TripResponse>>.Ok(
            trips.Select(t => MapToTripResponse(t, t.ServiceRoute.Name,
                MapToEventResponses(t.TripEvents))).ToList());
    }

    // ── Private Mappers ───────────────────────────────────
    private static TripResponse MapToTripResponse(Trip trip, string routeName, List<TripEventResponse> events) => new()
    {
        Id = trip.Id,
        RouteId = trip.ServiceRouteId,
        RouteName = routeName,
        TripType = trip.TripType.ToString(),
        Status = trip.Status.ToString(),
        StartedAt = trip.StartedAt,
        EndedAt = trip.EndedAt,
        Events = events
    };

    private static List<TripEventResponse> MapToEventResponses(ICollection<TripEvent> events) =>
        events.Select(e => new TripEventResponse
        {
            StudentId = e.StudentId,
            StudentName = e.Student?.FullName ?? string.Empty,
            EventType = e.EventType.ToString(),
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            Timestamp = e.Timestamp
        }).ToList();
}