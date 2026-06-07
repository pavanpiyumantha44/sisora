using System.Text.Json;
using Sisora.API.Models.DTOs.Responses;
using Sisora.API.Services.Interfaces;
using StackExchange.Redis;

namespace Sisora.API.Services;

public class LocationService : ILocationService
{
    private readonly IConnectionMultiplexer _redis;
    private const int TtlSeconds = 15;

    public LocationService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task SetLiveLocationAsync(Guid tripId, double latitude, double longitude)
    {
        var db = _redis.GetDatabase();

        var payload = JsonSerializer.Serialize(new
        {
            TripId = tripId,
            Latitude = latitude,
            Longitude = longitude,
            Timestamp = DateTime.UtcNow
        });

        // overwrite every ping, TTL resets to 15s
        await db.StringSetAsync(
            GetKey(tripId),
            payload,
            TimeSpan.FromSeconds(TtlSeconds)
        );
    }

    public async Task<LiveLocationResponse?> GetLiveLocationAsync(Guid tripId)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(GetKey(tripId));

        if (value.IsNullOrEmpty)
        {
            // key expired = signal lost
            return new LiveLocationResponse
            {
                TripId = tripId,
                IsSignalLost = true
            };
        }

        var data = JsonSerializer.Deserialize<JsonElement>((string)value!);

        return new LiveLocationResponse
        {
            TripId = tripId,
            Latitude = data.GetProperty("Latitude").GetDouble(),
            Longitude = data.GetProperty("Longitude").GetDouble(),
            Timestamp = data.GetProperty("Timestamp").GetDateTime(),
            IsSignalLost = false
        };
    }

    public async Task ClearLiveLocationAsync(Guid tripId)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(GetKey(tripId));
    }

    private static string GetKey(Guid tripId) => $"sisora:live:{tripId}";
}