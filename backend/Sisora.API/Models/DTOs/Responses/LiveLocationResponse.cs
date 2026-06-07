namespace Sisora.API.Models.DTOs.Responses;

public class LiveLocationResponse
{
    public Guid TripId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsSignalLost { get; set; }
}