namespace Sisora.API.Models.DTOs.Responses;

public class TripResponse
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string TripType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<TripEventResponse> Events { get; set; } = new();
}