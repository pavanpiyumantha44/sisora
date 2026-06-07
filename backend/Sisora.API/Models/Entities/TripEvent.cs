using backend.Models.Enums;

namespace Sisora.API.Models.Entities;

public class TripEvent : BaseEntity
{
    public Guid TripId { get; set; }
    public Guid StudentId { get; set; }
    public TripEventType EventType { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Trip Trip { get; set; } = null!;
    public Student Student { get; set; } = null!;
}